# Troubleshooting

#### I can't get the app to connect to the Unity Editor
<br />

1. On your mobile device, make sure that the network permission is granted:

  a. Go to the mobile device Settings page.  
  b. Scroll down to the app you're using: Virtual Camera or Face Capture.  
  c. Select the app and verify that Local Network is enabled.
<br /><br />

2. On your mobile device and Unity Editor workstation, make sure that:  
  * WiFi is enabled.
  * Both are connected to the same network.
<br /><br />

3. In the Unity Editor, make sure you have:  
  * Created a [connection server](connection-device.md) in the Connections window.
  * Started the server by clicking **Start** in this window.
<br /><br />

4. On **Windows** only:

  * If you're connected to a trusted private WiFi network such as a home or company network:
    * In Windows settings, verify that your WiFi network profile is [set to Private](connection-network.md#private-wi-fi-network-setup).
    * In the Unity Editor, in the Connections window, if you see a "Firewall is not configured" warning, select **Configure Firewall**.
    * If you're using a 3rd party software instead of the default Windows Defender firewall, you need to manually [configure a firewall rule](connection-network.md#manual-firewall-rule-configuration) in this 3rd party software.
<br /><br />

  * If you're connected to a public WiFi network, you have two options:
    * Manually [configure a firewall rule](connection-network.md#manual-firewall-rule-configuration) for this public network (Unity's automatic firewall configuration has no effect on public network firewall settings), OR
    * Switch your WiFi network profile to [Private](connection-network.md#private-wi-fi-network-setup).<br />**Warning:** This last solution has **security risks** if you can't fully trust the public network you're using.
<br /><br />

5. If you still can't connect:

  a. On the mobile device, switch to **Manual** mode to manually enter the server port and IP address.  
  b. In the Unity Editor, see the Connections window to know the server's **Port** and the **Available Interfaces** (IP addresses).  
  c. On the mobile device, try all of the different IP address values until you find one that works.
<br /><br />

#### I'm connected but nothing is happening in the Unity Editor
<br />

Ensure that:
* There is a **Take Recorder** object in your scene (**GameObject > Live Capture > Take Recorder**).
* There is a device to handle incoming data streams by adding a device to **Capture Devices** on the **Take Recorder** component in your scene.
* The device game object (e.g. "New VirtualCameraDevice") is a child of a game object with a **Take Recorder** component and the device is assigned in the **Capture Devices** list of the **Take Recorder**.
* The toggle to the left of the device in the **Capture Devices** section of the **Take Recorder** component is enabled.
* **Live** mode is enabled on the **Take Recorder**.
* In the case of using the Virtual Camera or Face Capture apps, ensure that the device has an **Actor** assigned.
