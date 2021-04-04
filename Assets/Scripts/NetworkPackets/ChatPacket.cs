using MessagePack;

[MessagePackObject]
public class ChatPacket
{
    [Key(0)]
    public string author{ get; set; }
    [Key(1)]
    public string entry{ get; set; }
}