using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ClientService : MonoBehaviour
{
    private TcpClient client;
    NetworkStream stream;

    private Thread clientThread;
    [SerializeField] private ChatUI chatUI;
    void Start()
    {   // Disabled as Join Server button calls this function
        //Invoke(nameof(CreateClient), 1f);
    }
        //Connects to entered ServerIP on inputfield on port 13000 then starts recieving thread
    public void CreateClient(string ServerIP) 
    {
        client = new TcpClient();
        Debug.Log("Client created");
        client.Connect(ServerIP, 13000); // as above
        Debug.Log("Client Connect request");
        stream = client.GetStream();
        clientThread = new Thread(receiverThread);
        clientThread.Start();
        
    }
    //Recieves messages from server and queues UI updates on unitys main thread
    private void receiverThread()
    {
        while(client.Connected)
        {
            //receive message
            byte[] data = new byte[1024];
            string msg = string.Empty;
            int bytes = stream.Read(data, 0, data.Length);
            msg = Encoding.ASCII.GetString(data, 0, bytes);
            Debug.Log("CLIENT :: Message Received : " + msg);
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                chatUI.AddMessage("Server: " + msg);
            });
        }
        Thread.Sleep(100);
    }
    //Converts message to ASCII bytes and sends to server 
    public void Send(string message)
    {
        byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
        stream.Write(data, 0, data.Length);
    }
    //Handles cleanup
    private void OnDisable()
    {
        client.Close();
        stream.Close();
        clientThread.Abort();
    }
}
