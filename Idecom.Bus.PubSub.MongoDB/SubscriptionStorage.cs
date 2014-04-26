using System.Collections.Generic;
using Idecom.Bus.Addressing;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Interfaces.Addons.PubSub;

namespace Idecom.Bus.PubSub.MongoDB
{
    public class SubscriptionStorage: ISubscriptionStorage, IBeforeBusStarted, IBeforeBusStopped
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }

        public IEnumerable<Address> GetSubscribersFor(object message)
        {
            return new List<Address>();
        }

        public void BeforeBusStarted()
        {
            
        }

        public void BeforeBusStopped()
        {
            
        }
    }
}