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
            packet   = greet.Serialize()
        };

        _server.SendAll(msg.Serialize());
    }
}