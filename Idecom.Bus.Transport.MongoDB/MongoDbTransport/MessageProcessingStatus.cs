namespace Idecom.Bus.Transport.MongoDB.MongoDbTransport
{
    public enum MessageProcessingStatus
    {
        AwaitingDispatch = 0,
        ReceivedByConsumer = 1,
        PermanentlyFailed = 3
    }
}