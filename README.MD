Telegraph is built on [Telepathy](https://github.com/vis2k/Telepathy) and [MessagePack](https://github.com/neuecc/MessagePack-CSharp).

I also made a custom command console, inspired by Quantum Console from unity asset store.

Project contains a chat sample for server and client.

`ServerApplicationStartup.cs`

```csharp
using System;
using UnityEngine;
using CommandConsole;

public class ServerApplicationStartup : MonoBehaviour
{
    private Server _server;
    private MessageHandler _handler;
    private CommandConsole.Console _console;

    private void Start()
    {
        Application.targetFrameRate = 60;

        _server = new Server();
        _server.Start(7777);

        _server.ClientConnectedEvent    += OnClientConnect;
        _server.MessageReceivedEvent    += OnMessage;
        _server.ClientDisconnectedEvent += OnClientDisconnect;

        _handler = new MessageHandler();
        _handler.AddHandler("chat", HandleChatMessage);

        _console = new CommandConsole.Console();
        _console.Initialize();
        _console.Register("greet_all", ClientGreeter);
    }

    private void ListenConsoleInput()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Tab))
        {
            _console.Show();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _console.Hide();
        }
    }

    //  event callbacks
    private void OnClientConnect(int connId)
    {
        var greet = new GreetPacket()
        {
            id = connId,
            greetMessage = "Server connection success. Welcome to chat."
        };

        var msg = new Message()
        {
            header = "greet",
            packet   = greet.Serialize()
        };

        _server.Send(connId, msg.Serialize());
    }

    private void OnMessage(int connId, ArraySegment<byte> data)
    {
        _handler.Handle(data);
    }

    private void OnClientDisconnect(int connId)
    {
        Debug.Log("Client connection lost. connId: " + connId);
    }

    private void OnApplicationQuit()
    {
        _server.Stop();
    }

    //  handlers
    private void HandleChatMessage(ArraySegment<byte> data)
    {
        var chat = data.Deserialize<ChatPacket>();

        Debug.Log("received chat, author: " + chat.author);

        var msg = new Message()
        {
            header = "chat",
            packet = data
        };
        
        _server.SendAll(msg.Serialize());
    }

    //  tick server
    private void Update()
    {
        ListenConsoleInput();
        _server.Tick(100);
    }

    //  console commands
    private void ClientGreeter(string[] args)
    {
        var greet = new GreetPacket()
        {
            id = -1,
            greetMessage = "Server watch you. Take care."
        };

        var msg = new Message()
        {
            header = "greet",
            packet = greet.Serialize()
        };

        _server.SendAll(msg.Serialize());
    }
}
```
`ClientApplicationStartup.cs`

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientApplicationStartup : MonoBehaviour
{
    public string ip = "127.0.0.1";
    public int port = 7777;
    public Transform canvasParent;
    public GameObject loginPrefab;
    public GameObject chatPrefab;

    private string _clientNickname;
    private ChatPanel _chatPanel;

    private Client _client;
    private MessageHandler _handler;

    private void Start()
    {
        Application.targetFrameRate = 60;

        var login = Instantiate(loginPrefab, canvasParent).GetComponent<LoginPanel>();
        login.inputField.onEndEdit.AddListener(input => _clientNickname = input );
        login.connectButton.onClick.AddListener(() => Connect());
    }

    private void CreateChatPanel()
    {
        _chatPanel = Instantiate(chatPrefab, canvasParent).GetComponent<ChatPanel>();
        _chatPanel.chatLogText.text = "";
        _chatPanel.inputField.onEndEdit.AddListener(OnChatInput);
        _chatPanel.inputField.ActivateInputField();
    }
    
    private void Connect()
    {
        CreateChatPanel();

        _client = new Client();
        _client.Start(ip, port);
        _client.OnMessageReceived   += MessageReceived;
        
        _handler = new MessageHandler();
        _handler.AddHandler("chat",     HandleChatMessage);
        _handler.AddHandler("greet",    GreetHandleMessage);

    }

    //  event callbacks
    private void MessageReceived(ArraySegment<byte> msg)
    {
        _handler.Handle(msg);
    }

    private void OnChatInput(string input)
    {
        if (!string.IsNullOrEmpty(input) && !string.IsNullOrWhiteSpace(input))
        {
            var chat = new ChatPacket()
            {
                author = _clientNickname,
                entry  = input
            };

            var msg = new Message()
            {
                header = "chat",
                packet   = chat.Serialize()
            };

            _client.Send(msg.Serialize());

            _chatPanel.inputField.text = "";
            _chatPanel.inputField.ActivateInputField();
        }
    }

    private void OnApplicationQuit()
    {
        _client.Disconnect();
    }

    //  handlers
    private void HandleChatMessage(ArraySegment<byte> chatData)
    {
        var chat = chatData.Deserialize<ChatPacket>();

        //  log to view
        _chatPanel.AddLog(chat.author + ": " + chat.entry);
    }

    private void GreetHandleMessage(ArraySegment<byte> chatData)
    {
        var greet = chatData.Deserialize<GreetPacket>();

        _chatPanel.AddLog(greet.greetMessage);
        _chatPanel.AddLog("Connection id: " + greet.id);
    }

    //  tick client
    private void Update()
    {
        _client.Tick(100);
    }
}
```