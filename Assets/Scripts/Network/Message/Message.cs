using System;
using MessagePack;

[MessagePackObject]
public class Message
{
    [Key(0)]
    public string header{ get; set; }
    [Key(1)]
    public ArraySegment<byte> packet{ get; set; }
}