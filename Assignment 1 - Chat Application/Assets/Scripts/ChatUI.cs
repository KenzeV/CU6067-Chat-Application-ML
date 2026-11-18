using TMPro;
using UnityEngine;

public class ChatUI : MonoBehaviour
{
    //Variables and serialized fields for Unity inspector
    private bool isServer;

    [Header("Panels")]
    [SerializeField] private GameObject connectionPanel;
    [SerializeField] private GameObject chatPanel;

    [Header("Connection Controls")]
    [SerializeField] private TMP_InputField serverIPInput;
    [SerializeField] private TMP_Text statusText;

    [Header("Chat Controls")]
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private TMP_Text chatHistoryText;

    [Header("Network Services")]
    [SerializeField] private ServerService serverService;
    [SerializeField] private ClientService clientService;
    

    //Shows connection screen 

    private void Start()
    {

        messageInput.text = "";
        messageInput.interactable = false;

        connectionPanel.SetActive(true);
        chatPanel.SetActive(false);

        statusText.text = "Status: Not connected";

        messageInput.lineType = TMP_InputField.LineType.SingleLine; 
        messageInput.onSubmit.AddListener(text => SendMessage());
    }
    //Begins hosting and opens chat screen while waiting for client to connect
    public void HostServer()
    {
        serverService.CreateServer(serverIPInput.text.Trim());
        isServer = true;
        ShowChat();
        AddMessage("Server started. Waiting for a client...");
        statusText.text = "Status: Hosting - waiting for a client...";
    }
    //Connects to server IP and opens chatscreen
    public void JoinServer()
    {
      
        string serverIP = serverIPInput.text.Trim();

       
        clientService.CreateClient(serverIP);
        isServer = false;
        ShowChat();
        ConnectionReady();
        statusText.text = "Status: Connected to " + serverIP;

    }

    //Switches to chatpanel and clears previous chat history
    private void ShowChat()
    {
        connectionPanel.SetActive(false);
        chatPanel.SetActive(true);

        chatHistoryText.text = "";
    }
    //Enables message input and displays connection confirmation
    public void ConnectionReady()
    {
        messageInput.interactable = true;
        AddMessage("Connected. You can send messages.");
    }
    //Adds message to chat
    public void AddMessage(string message)
    {
        chatHistoryText.text += message + "\n";
    }
    //Sends message as either host or client, displays and clears input from input field
    public void SendMessage()
    {
        string message = messageInput.text.Trim();

       
        if (message == "" || !messageInput.interactable)
        {
            return;
        }

        if (isServer)
        {
            serverService.Send(message);
        }
        else
        {
            clientService.Send(message);
        }

        
        AddMessage("Me: " + message + " *");
        messageInput.text = "";
    }
}
