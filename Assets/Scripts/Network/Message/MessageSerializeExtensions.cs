using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using MessagePack;

public static class MessageSerializeExtensions
{
    public static ArraySegment<byte> Serialize(this object obj)
    {   
        var data = MessagePackSerializer.Serialize(obj);
        var segment = new ArraySegment<byte>(data);
        return segment;
    }

    public static T Deserialize<T>(this ArraySegment<byte> segment)
    {   
        return MessagePackSerializer.Deserialize<T>(segment.Array);;
    }
}