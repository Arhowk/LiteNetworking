
//Attach this Button in the Inspector of your GameObject.

//In Play Mode, click the Button to connect. If the connection works, the details are output to the Console window. If there is an error, the error is output to the console.

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Example : MonoBehaviour
{
    int m_ServerSocket;

    HostTopology m_HostTopology;

    //These are the Buttons that start the client and server, and the Button for sending messages
    //Assure that you assign these in the Inspector before testing
    public Button m_ServerButton;

    void Start()
    {
        //Set up the Connection Configuration which holds channel information
        ConnectionConfig config = new ConnectionConfig();

        //Create a new Host information based on the configuration created, and the maximum connections allowed (20)
        m_HostTopology = new HostTopology(config, 20);
        //Initialise the NetworkTransport
        NetworkTransport.Init();

        //Call the ServerButton function when you click the server Button
        m_ServerButton.onClick.AddListener(ServerButton);
    }

    void Update()
    {
        /*//These are the variables that are replaced by the incoming message
        int outHostId;
        int outConnectionId;
        int outChannelId;
        byte[] buffer = new byte[1024];
        int receivedSize;
        byte error;

        //Set up the Network Transport to receive the incoming message, and decide what type of event
        NetworkEventType eventType = NetworkTransport.Receive(out outHostId, out outConnectionId, out outChannelId, buffer, buffer.Length, out receivedSize, out error);

        switch (eventType)
        {
            //Use this case when there is a connection detected
            case NetworkEventType.ConnectEvent:
                {
                    //Call the function to deal with the received information
                    OnConnect(outHostId, outConnectionId, (NetworkError)error);
                    break;
                }

            case NetworkEventType.Nothing:
                break;

            default:
                //Output the error
                Debug.LogError("Unknown network message type received: " + eventType);
                break;
        }*/
    }

    //This function is called when a connection is detected
    void OnConnect(int hostID, int connectionID, NetworkError error)
    {
        //Output the given information to the console
        Debug.Log("OnConnect(hostId = " + hostID + ", connectionId = "
            + connectionID + ", error = " + error.ToString() + ")");
    }

    void ServerButton()
    {
        byte error;
        //Open the sockets for sending and receiving the messages on port 54321
        m_ServerSocket = NetworkTransport.AddHost(m_HostTopology, 54321);
        //Connect the "server"
        NetworkTransport.Connect(m_ServerSocket, "127.0.0.1", 54322, 0, out error);
        //Check for if there is an error
        if ((NetworkError)error != NetworkError.Ok)
        {
            //Output this message in the console with the Network Error
            Debug.Log("There was this error : " + (NetworkError)error);
        }
        //Otherwise if no errors occur, output this message to the console
        else Debug.Log("Connected : " + (NetworkError)error);
    }
}