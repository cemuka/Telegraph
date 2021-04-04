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
