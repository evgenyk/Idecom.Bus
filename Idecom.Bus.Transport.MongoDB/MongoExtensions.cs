namespace Idecom.Bus.Transport.MongoDB
{
    using Addressing;

    internal static class MongoExtensions
    {
        public static MongoTransportMessageEntity.MongoAddress ToMongoAddress(this Address address)
        {
            return new MongoTransportMessageEntity.MongoAddress(address.Queue, address.Datacenter);
        }

        public static Address ToAddress(this MongoTransportMessageEntity.MongoAddress mongoAddress)
        {
            return new Address(mongoAddress.Queue, mongoAddress.Datacenter);
        }
    }
}