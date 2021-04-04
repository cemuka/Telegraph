using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server
{
    public event Action<int>                        ClientConnectedEvent;
    public event Action<int, ArraySegment<byte>>    MessageReceivedEvent;
    public event Action<int>                        ClientDisconnectedEvent;

    private Telepathy.Server _server;

    public Server()
    {
        _server = new Telepathy.Server(1000);
    }

    public void Start(int port)
    {        
        _server.Start(port);
        _server.OnConnected     += OnClientConnected;
        _server.OnDisconnected  += OnClientDisconnected;
        _server.OnData          += OnClientData;

    }

    public void SendAll(ArraySegment<byte> data)
    {
        foreach (var c in _server.clients.Keys)
        {
            _server.Send(c, data);
        }
    }

    public void Send(int id, ArraySegment<byte> data)
    {
        _server.Send(id, data);
    }

    private void OnClientConnected(int id)
    {
        ClientConnectedEvent?.Invoke(id);
    }

    private void OnClientDisconnected(int id)
    {
        ClientDisconnectedEvent?.Invoke(id);
    }

    private void OnClientData(int id, ArraySegment<byte> data)
    {
        MessageReceivedEvent?.Invoke(id, data);
    }

    public void Stop()
    {
        _server.Stop();
    }

    public void Tick(int processLimit)
    {
        _server.Tick(processLimit);
    }
}