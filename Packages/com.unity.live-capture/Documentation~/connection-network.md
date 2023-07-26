# Set up your network and firewall

To enable a proper connection between the [Client Devices](data-capture-process.md#client-device) and the Unity Editor:

* Make sure the devices and Unity Editor workstation have access to the **same network**.

* **Disable** any active **VPNs**.

* On Windows 10:
  * Make sure the **Wi-Fi** network you are using is [set to **Private**](#private-wi-fi-network-setup).
  * Set up your **firewall** to allow your Unity Editor program to get **inbound connections** from external apps of your local network
    * In the Unity Editor, use the Connections window to [automatically configure your firewall](#automatic-firewall-configuration).
    * If you still can’t connect, [manually configure your firewall](#manual-firewall-rule-configuration). with a dedicated rule.

## Private Wi-Fi network setup

1.  In the Windows Settings, select **Network & Internet**.

    ![](images/network-windows-settings.png)

2.  In the left panel, select **WiFi**, and then click on your connection on the right.

    ![](images/network-wifi.png)

3.  In **Network profile**, select **Private**.

    ![](images/network-private.png)

## Automatic firewall configuration

1.  Open the **Connections** window by selecting **Menu** > **Window** > **Live Capture** > **Connections**.

2.  If your firewall is not yet configured for Live Capture, the Connections window displays a "Firewall is not configured" message along with a **Configure Firewall** button. Click on this button.

3.  If a user account control message box pops up asking for administrator privileges, you must grant permission.

## Manual firewall rule configuration

> **Note:** The following instructions apply to the default Windows Defender firewall. If you're using any other 3rd party firewall, see its documentation to reproduce the same configuration.

1.  In the Windows **Control Panel**, go to **System and Security** > **Windows Defender Firewall**.

2.  In the left menu, select **Advanced Settings**.

    ![](images/firewall-advanced-settings.png)

3.  In the left pane, first select **Inbound Rules**.

    ![](images/firewall-inbound-rules.png)

4.  In the right pane, select **New Rule**.

    ![](images/firewall-new-rule.png)

5.  Perform the Program rule configuration steps through the New Inbound Rule Wizard according to your needs (see below).  

### Configuring a Program rule

Select **Next** each time to go to the next step.

| **Step**  | **Setting** |
|-----------|-------------|
| Rule Type | • Select **Program** (default). |
| Program   | • Select **This program path:** and then **Browse** to the `Unity.exe` file that corresponds to the Unity Editor that you want to connect the apps to. |
| Action    | • Select **Allow the connection** (default). |
| Profile   | • Select the profile that corresponds to the network you're using, and keep the other profiles unselected.<br /><br />For example, for a private network, select **Private** and keep **Domain** and **Public** unselected. |
| Name      | • Enter **Unity Live Capture**, for example. |
