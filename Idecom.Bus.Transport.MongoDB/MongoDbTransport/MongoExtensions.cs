using Idecom.Bus.Addressing;

namespace Idecom.Bus.Transport.MongoDB.MongoDbTransport
{
    public static class MongoExtensions
    {
        public static MongoTransportMessage.MongoAddress ToMongoAddress(this Address address)
        {
            return new MongoTransportMessage.MongoAddress(address.Queue, address.Datacenter);
        }

        public static Address ToAddress(this MongoTransportMessage.MongoAddress mongoAddress)
        {
            return new Address(mongoAddress.Queue, mongoAddress.Datacenter);
        }
    }
}