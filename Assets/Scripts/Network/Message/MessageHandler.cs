using System;
using System.Collections.Generic;

public class MessageHandler
{
    private Dictionary<string, Action<ArraySegment<byte>>> _handlers;

    public MessageHandler()
    {
        _handlers = new Dictionary<string, Action<ArraySegment<byte>>>();
    }

    public void AddHandler(string type, Action<ArraySegment<byte>> func)
    {
        _handlers.Add(type, func);
    }

    public void Handle(ArraySegment<byte> data)
    {
        var msg = data.Deserialize<Message>();
        _handlers[msg.header](msg.packet);
    }
}