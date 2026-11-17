using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
//Hosts TCP Connection and sends/recieves chat messages
public class ServerService : MonoBehaviour
{
    private IPAddress serverIp = IPAddress.Parse("127.0.0.1");
    private int port = 13000;
    private TcpListener server;
    private TcpClient client;
    NetworkStream stream;

    bool isRunning = false;
    //Tracking if server running and if client connected
    private Thread serverThread;
    bool isClientConnected = false;
    [SerializeField] private ChatUI chatUI;

    void Start()
    {   //Disabled as Host button starts this function
        //CreateServer();
    }
    //Starts TCP listener and thread used to accept a client and receive messages
    public void CreateServer()
    {
        if (isRunning)
        {
            return;
        }
        server = new TcpListener(serverIp, port);
        Debug.Log("SERVER :: Created");
        server.Start();
        Debug.Log("SERVER :: Started");
        isRunning = true;
        serverThread = new Thread(receiverThread);
        serverThread.Start();
    }

    private void receiverThread()
    {    //Checks for client connection and incoming messages while server is running then queues UI updates for main thread
        while(isRunning)
        {
            if(isClientConnected == false)
            {
                client = server.AcceptTcpClient();
                Debug.Log("CLIENT CONNECTED");
                isClientConnected = true;
                stream = client.GetStream();
                //Queues UI Update to run on unity's main thread
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    chatUI.ConnectionReady();
                });
            }
            else
            {
                //receive message
                byte[] data = new byte[1024];
                string msg = string.Empty;
                int bytes = stream.Read(data, 0, data.Length);
                msg = Encoding.ASCII.GetString(data, 0, bytes);
                Debug.Log("SERVER :: Message Received : " + msg);
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    chatUI.AddMessage("Client: " + msg);
                });
                //Send("DELIVERED " + msg); //Debugging for server ACK
            }
            Thread.Sleep(100);
        }
    }
    //Converts message to ASCII bytes and sends it to connected client
    public void Send(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        stream.Write(data, 0, data.Length);
    }
    //Handles Cleanup
    private void OnDisable()
    {
        stream.Close();
        server.Stop();
        isClientConnected = false;
        isRunning = false;
        serverThread.Abort();
    }
}
