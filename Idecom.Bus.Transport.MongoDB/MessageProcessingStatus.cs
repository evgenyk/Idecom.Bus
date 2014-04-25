namespace Idecom.Bus.Transport.MongoDB
{
    public enum MessageProcessingStatus
    {
        AwaitingDispatch = 0,
        ReceivedByConsumer = 1,
        PermanentlyFailed = 3
    }
}