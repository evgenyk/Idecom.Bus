namespace Idecom.Bus.Transport.MongoDB
{
    internal enum MessageProcessingStatus
    {
        AwaitingDispatch = 0,
        ReceivedByConsumer = 1,
        PermanentlyFailed = 3
    }
}