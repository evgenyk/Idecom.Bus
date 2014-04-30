namespace Idecom.Bus.Transport
{
    public enum MessageIntent
    {
        Send = 0,
        SendLocal = 1,
        Reply = 10,
        Publish = 20
    }
}