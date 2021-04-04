using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class Client
{
    public event Action                     OnConnected;
    public event Action                     OnDisconnected;
    public event Action<ArraySegment<byte>> OnMessageReceived;

    private Telepathy.Client _client;

    public Client()
    {
        _client = new Telepathy.Client(1000);
    }

    public void Start(string ip, int port)
    {
        _client.OnConnected     += OnConnect;
        _client.OnDisconnected  += OnDisconnect;
        _client.OnData          += MessageRecieve;

        _client.Connect(ip, port);
    }

    private void OnConnect()
    {
        OnConnected?.Invoke();
    }

    private void OnDisconnect()
    {
        OnDisconnected?.Invoke();
    }

    private void MessageRecieve(ArraySegment<byte> data)
    {
        OnMessageReceived?.Invoke(data);
    }
    
    public void Send(ArraySegment<byte> data)
    {
        _client.Send(data);
    }

    public void Disconnect()
    {
        _client.Disconnect();
    }

    public void Tick(int processLimit)
    {
        _client.Tick(processLimit);
    }
}