using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;

namespace Unity.LiveCapture.Networking
{
    /// <summary>
    /// A class containing methods for common networking related operations.
    /// </summary>
    public static class NetworkUtilities
    {
        static readonly ConcurrentDictionary<IPAddress, uint> s_AddressBits = new ConcurrentDictionary<IPAddress, uint>();

        /// <summary>
        /// Get the online IPv4 addresses from all network interfaces in the system.
        /// </summary>
        /// <param name="includeLoopback">Include any addresses on the loopback interface.</param>
        /// <returns>A new array containing the available pairs of IP address and the <see cref="NetworkInterface"/> that it belongs to.</returns>
        public static (IPAddress IP, NetworkInterface Interface)[] GetIPInterfaces(bool includeLoopback)
        {
            var addresses = new List<(IPAddress, NetworkInterface)>();

            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (!includeLoopback && networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                switch (networkInterface.OperationalStatus)
                {
                    case OperationalStatus.Up:
                        break;
                    case OperationalStatus.Unknown:
                        // On Linux the loopback interface reports as unknown status, so we get it anyways
                        if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                            break;
                        continue;
                    default:
                        continue;
                }

                foreach (var ip in networkInterface.GetIPProperties().UnicastAddresses)
                {
                    var address = ip.Address;

                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        addresses.Add((address, networkInterface));
                    }
                }
            }

            return addresses.ToArray();
        }

        /// <summary>
        /// Get the online IPv4 addresses from all network interfaces in the system.
        /// </summary>
        /// <param name="includeLoopback">Include any addresses on the loopback interface.</param>
        /// <returns>A new array containing the available IP addresses.</returns>
        public static IPAddress[] GetIPAddresses(bool includeLoopback)
        {
            return GetIPInterfaces(includeLoopback).Select(x => x.IP).ToArray();
        }

        /// <summary>
        /// Gets the interface used to communicate with the specified remote.
        /// </summary>
        /// <param name="remoteEndPoint">The end point to connect to.</param>
        /// <returns>The IP address of the interface to use.</returns>
        public static IPAddress GetRoutingInterface(IPEndPoint remoteEndPoint)
        {
            // The routing table is the most correct method to determine the interface to use, but
            // in case there is an error we call back to an alternate method that is less reliable.
            try
            {
                return QueryRoutingInterface(null, remoteEndPoint);
            }
            catch (Exception)
            {
                return FindClosestAddresses(remoteEndPoint).localAddress;
            }
        }

        /// <summary>
        /// Looks up the interface used to communicate with the specified remote from the routing table.
        /// </summary>
        /// <param name="socket">The socket used for the lookup query. If null, a temporary socket is used.</param>
        /// <param name="remoteEndPoint">The end point to connect to.</param>
        /// <returns>The IP address of the interface to use.</returns>
        public static IPAddress QueryRoutingInterface(Socket socket, IPEndPoint remoteEndPoint)
        {
            var address = remoteEndPoint.Serialize();

            var remoteAddrBytes = new byte[address.Size];
            var localAddrBytes = new byte[address.Size];

            for (var i = 0; i < address.Size; i++)
            {
                remoteAddrBytes[i] = address[i];
            }

            if (socket != null)
            {
                socket.IOControl(IOControlCode.RoutingInterfaceQuery, remoteAddrBytes, localAddrBytes);
            }
            else
            {
                using var tempSocket = CreateSocket(ProtocolType.Udp);
                tempSocket.IOControl(IOControlCode.RoutingInterfaceQuery, remoteAddrBytes, localAddrBytes);
            }

            for (var i = 0; i < address.Size; i++)
            {
                address[i] = localAddrBytes[i];
            }

            return ((IPEndPoint)remoteEndPoint.Create(address)).Address;
        }

        /// <summary>
        /// Finds the local IP address and remote IP address that share the largest prefix.
        /// </summary>
        /// <remarks>
        /// IP addresses that share a prefix are likely to be on the same subnet.
        /// </remarks>
        /// <param name="remoteEndPoints">The remote IP addresses to pick from.</param>
        /// <returns>A tuple containing the most similar local IP address and remote IP address pair, or
        /// IPAddress.Any and null if no suitable pair was found.</returns>
        public static (IPAddress localAddress, IPEndPoint remoteEndPoint) FindClosestAddresses(params IPEndPoint[] remoteEndPoints)
        {
            // only match local host exactly
            foreach (var remoteEndPoint in remoteEndPoints)
            {
                if (remoteEndPoint.Equals(IPAddress.Loopback))
                    return (IPAddress.Loopback, remoteEndPoint);
            }

            // find the most similar non-loopback interface address
            var bestMatchLength = 0;
            var bestLocalIP = IPAddress.Any;
            var bestRemote = default(IPEndPoint);

            foreach (var localIP in GetIPAddresses(false))
            {
                foreach (var remoteEndPoint in remoteEndPoints)
                {
                    var matchLength = CompareIPAddresses(localIP, remoteEndPoint.Address);

                    if (bestMatchLength < matchLength)
                    {
                        bestMatchLength = matchLength;
                        bestLocalIP = localIP;
                        bestRemote = remoteEndPoint;
                    }
                }
            }

            return (bestLocalIP, bestRemote);
        }

        /// <summary>
        /// Gets an IPv4 address as an integer.
        /// </summary>
        /// <param name="address">An IPv4 address to get the bits for.</param>
        /// <returns>The IP address bits.</returns>
        public static uint GetAddressBits(IPAddress address)
        {
            if (address.AddressFamily != AddressFamily.InterNetwork)
                throw new ArgumentException($"{address.AddressFamily} addresses are not supported!", nameof(address));

            if (s_AddressBits.TryGetValue(address, out var bits))
                return bits;

            var bytes = address.GetAddressBytes();

            // the address is given in big-endian, so we need to convert back to the platform endianness
            if (BitConverter.IsLittleEndian)
            {
                bits = unchecked((uint)((bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3]));
            }
            else
            {
                bits = unchecked((uint)((bytes[3] << 24) | (bytes[2] << 16) | (bytes[1] << 8) | bytes[0]));
            }

            s_AddressBits.TryAdd(address, bits);
            return bits;
        }

        /// <summary>
        /// Counts the matching leading bits of two IPv4 addresses.
        /// </summary>
        /// <param name="a">The first IP address.</param>
        /// <param name="b">The second IP address.</param>
        /// <returns>The length of the shared prefix.</returns>
        public static int CompareIPAddresses(IPAddress a, IPAddress b)
        {
            var aBits = GetAddressBits(a);
            var bBits = GetAddressBits(b);

            var matchingBits = 0;
            do
            {
                var shift = 31 - matchingBits;

                if ((aBits >> shift) != (bBits >> shift))
                    break;

                matchingBits++;
            }
            while (matchingBits < 32);

            return matchingBits;
        }

        /// <summary>
        /// Gets the MAC address of the network interface corresponding to a local IP address.
        /// </summary>
        /// <param name="address">A local IP address.</param>
        /// <returns>The MAC address, or null if no interfaces match the provided address.</returns>
        public static PhysicalAddress GetPhysicalAddress(IPAddress address)
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                var properties = networkInterface.GetIPProperties();

                if (properties.UnicastAddresses.Any(ip => ip.Address.Equals(address)))
                {
                    return networkInterface.GetPhysicalAddress();
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a new socket.
        /// </summary>
        /// <param name="protocol">The protocol to use on the created socket.</param>
        /// <returns>The new socket instance.</returns>
        public static Socket CreateSocket(ProtocolType protocol)
        {
            SocketType type;

            switch (protocol)
            {
                case ProtocolType.Tcp:
                    type = SocketType.Stream;
                    break;
                case ProtocolType.Udp:
                    type = SocketType.Dgram;
                    break;
                default:
                    throw new ArgumentException("Only TCP or UDP are supported", nameof(protocol));
            }

            return new Socket(AddressFamily.InterNetwork, type, protocol);
        }

        /// <summary>
        /// Cleanly closes a socket. May block for as long as the provided timeout.
        /// </summary>
        /// <param name="socket">The socket to close.</param>
        /// <param name="timeout">The timeout in seconds to wait for the connection
        /// to rend any remaining data then be cleanly shut down.</param>
        public static void DisposeSocket(Socket socket, int timeout = 1)
        {
            if (socket != null)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException e)
                {
                    switch (e.SocketErrorCode)
                    {
                        case SocketError.NotConnected:
                        case SocketError.ConnectionReset:
                        case SocketError.Disconnecting:
                            return;
                        default:
                            Debug.LogException(e);
                            return;
                    }
                }
                finally
                {
                    socket.Close(timeout);
                }
            }
        }

        /// <summary>
        /// Checks if a port is not used by any running program.
        /// </summary>
        /// <remarks>
        /// This checks the TCP and UCP ports on all available network interfaces.
        /// </remarks>
        /// <param name="port">The port number to check.</param>
        /// <returns>True if the port is free for each protocol on all network interfaces, false otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="port"/> is not a valid port number.</exception>
        /// <seealso cref="IsPortAvailable(ProtocolType, int)"/>
        /// <seealso cref="IsPortAvailable(ProtocolType, IPAddress, int)"/>
        public static bool IsPortAvailable(int port)
        {
            return IsPortAvailable(ProtocolType.Udp, port) && IsPortAvailable(ProtocolType.Tcp, port);
        }

        /// <summary>
        /// Checks if a port is not used by any running program.
        /// </summary>
        /// <remarks>
        /// This checks on all available network interfaces.
        /// </remarks>
        /// <param name="protocol">The protocol to check the ports for. Must be UDP or TCP.</param>
        /// <param name="port">The port number to check.</param>
        /// <returns>True if the port is free for the given protocol on all network interfaces, false otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="port"/> is not a valid port number.</exception>
        /// <seealso cref="IsPortAvailable(int)"/>
        /// <seealso cref="IsPortAvailable(ProtocolType, IPAddress, int)"/>
        public static bool IsPortAvailable(ProtocolType protocol, int port)
        {
            if (!IsPortValid(port, out var message))
                throw new ArgumentOutOfRangeException(nameof(port), port, message);

#if UNITY_WINDOWS
            return IsPortAvailableListeners(protocol, port);
#else
            return IsPortAvailableSocket(protocol, port);
#endif
        }

        /// <summary>
        /// Checks if a port is not used by any running program.
        /// </summary>
        /// <remarks>
        /// This checks the port for a specific protocol and network interface.
        /// </remarks>
        /// <param name="protocol">The protocol to check the ports for. Must be UDP or TCP.</param>
        /// <param name="address">The address of the port to check.</param>
        /// <param name="port">The port number to check.</param>
        /// <returns>True if the port is free, false otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="port"/> is not a valid port number.</exception>
        /// <seealso cref="IsPortAvailable(int)"/>
        /// <seealso cref="IsPortAvailable(ProtocolType, int)"/>
        public static bool IsPortAvailable(ProtocolType protocol, IPAddress address, int port)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));
            if (!IsPortValid(port, out var message))
                throw new ArgumentOutOfRangeException(nameof(port), port, message);

            var endPoint = new IPEndPoint(address, port);

#if UNITY_WINDOWS
            return IsPortAvailableListeners(protocol, endPoint);
#else
            return IsPortAvailableSocket(protocol, endPoint);
#endif
        }

        static bool IsPortAvailableSocket(ProtocolType protocol, int port)
        {
            foreach (var address in GetIPAddresses(true))
            {
                var endPoint = new IPEndPoint(address, port);

                if (!IsPortAvailableSocket(protocol, endPoint))
                {
                    return false;
                }
            }

            return true;
        }

        static bool IsPortAvailableSocket(ProtocolType protocol, EndPoint endPoint)
        {
            // Try to create a socket and check if it fails to bind since another
            // application is using the port. This is a workaround as IPGlobalProperties
            // is not implemented on all platforms.
            var socket = CreateSocket(protocol);

            try
            {
                // Other applications can enable sharing a port, but we want the socket
                // to fail to bind in those cases, so we ensure that we demand to be
                // the only application allowed to use the port we request.
                socket.ExclusiveAddressUse = true;
                socket.Bind(endPoint);
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
            finally
            {
                DisposeSocket(socket, 0);
            }
        }

        static bool IsPortAvailableListeners(ProtocolType protocol, int port)
        {
            try
            {
                foreach (var listener in GetActiveListeners(protocol))
                {
                    if (listener.Port == port)
                        return false;
                }

                return true;
            }
            catch (Exception e)
            {
                // on failure assume the port is free
                Debug.LogError($"Failed to access network information: {e}");
                return true;
            }
        }

        static bool IsPortAvailableListeners(ProtocolType protocol, EndPoint endPoint)
        {
            try
            {
                foreach (var listener in GetActiveListeners(protocol))
                {
                    if (listener.Equals(endPoint))
                        return false;
                }

                return true;
            }
            catch (Exception e)
            {
                // on failure assume the port is free
                Debug.LogError($"Failed to access network information: {e}");
                return true;
            }
        }

        static IPEndPoint[] GetActiveListeners(ProtocolType protocol)
        {
            // This is only implemented on Windows right now
            var ipProperties = IPGlobalProperties.GetIPGlobalProperties();

            switch (protocol)
            {
                case ProtocolType.Udp:
                    return ipProperties.GetActiveUdpListeners();
                case ProtocolType.Tcp:
                    return ipProperties.GetActiveTcpListeners();
                default:
                    throw new ArgumentOutOfRangeException(nameof(protocol), protocol, "Only UDP and TCP are supported!");
            }
        }

        /// <summary>
        /// Checks if a port number is valid to use.
        /// </summary>
        /// <param name="port">The port number to check.</param>
        /// <param name="message">Returns null if the port is suitable, otherwise it returns
        /// a message explaining why the port is not valid or recommended. May contain a value even
        /// if this method returns true.</param>
        /// <returns>True if the port number is strictly valid.</returns>
        public static bool IsPortValid(int port, out string message)
        {
            message = null;

            if (port < 0)
            {
                message = "Port numbers cannot be negative.";
                return false;
            }
            if (port == 0)
            {
                message = "Port 0 is a reserved port and cannot be used.";
                return false;
            }
            if (port > 65535)
            {
                message = "Port numbers cannot be larger than 65535.";
                return false;
            }
            if (port < 1024)
            {
                message = "Ports on range [1, 1023] are reserved for well-known services and cannot be used.";
                return false;
            }

            // This range encompasses the ephemeral port range used by all common OSs.
            // While this can be configured, the vast majority of systems use the defaults.
            if (port >= 32768)
            {
                message = "Ports on range [32768, 65535] are typically ephemeral and may be in use. It is recommended to use a port between 1024 and 32767.";
                return true;
            }

            return true;
        }
    }
}
