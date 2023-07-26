# Connect a Client Device to the Editor

Enable network connection between a [Client Device](data-capture-process.md#client-device) and the Unity Editor.

## Create a Connection

1. In the editor, open the **Connections** window: select **Menu** > **Window** > **Live Capture** > **Connections**.

2. Click on the **+** (plus) button and select the type of Connection to add.

3. Select the newly created Connection.

4. Set the **Port** if necessary.  
   **Note:** Only one Unity project using Live Capture can use a given port at a time. If the port is already in use by another program, including other Unity instances, a message appears, prompting you to select a free port.

5. Enable the Connection using its toggle button.

6. Connect to the Editor from the Client Device.

7. In the Unity Editor, verify you are connected: the device name must appear in the **Connections** window under the **Connected devices** section.

8.  Select **Auto Start on Play** to have the server automatically start when entering Play Mode.

## Companion app connection

To connect the Virtual Camera app or the Face Capture app to the Unity Editor:

1. In the Unity Editor, create a connection of type **Companion App Server** and enable it.
2. In the app launch screen, connect to the created server either with server discovery or manually.

### Connection with server discovery

On the app launch screen, select the Unity Editor Connection you want to use and press the **Connect** button.

### Manual connection

On the app launch screen, enter the IP address and Port number displayed in the **Connections** window under **Available Interfaces**.

If there are multiple IP addresses, try each of them until you get the connection.
