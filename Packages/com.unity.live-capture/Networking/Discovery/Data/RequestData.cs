using System;
using System.Runtime.InteropServices;

namespace Unity.LiveCapture.Networking.Discovery
{
    /// <summary>
    /// The data broadcast on the network to request servers matching a specific product name to send updated server information.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    struct RequestData
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.StringMaxLength + 1)]
        string m_ProductName;

        /// <summary>
        /// The name of the server application.
        /// </summary>
        public string ProductName => m_ProductName;

        /// <summary>
        /// Creates a new <see cref="RequestData"/> instance.
        /// </summary>
        /// <param name="productName">The name of the server application.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="productName"/>
        /// exceed <see cref="Constants.StringMaxLength"/> characters in length.</exception>
        public RequestData(string productName)
        {
            if (productName == null)
                throw new ArgumentNullException(nameof(productName));
            if (productName.Length > Constants.StringMaxLength)
                throw new ArgumentException($"String length of {productName.Length} exceeds maximum ({Constants.StringMaxLength} characters).", nameof(productName));

            m_ProductName = productName;
        }
    }
}
