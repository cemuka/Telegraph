using MessagePack;

[MessagePackObject]
public class GreetPacket
{
    [Key(0)]
    public int id;
    [Key(1)]
    public string greetMessage;
}