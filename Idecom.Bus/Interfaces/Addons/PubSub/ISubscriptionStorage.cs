using System.Collections.Generic;
using Idecom.Bus.Addressing;

namespace Idecom.Bus.Interfaces.Addons.PubSub
{
    public interface ISubscriptionStorage
    {
        IEnumerable<Address> GetSubscribersFor(object message);
        void Subscribe(Address subscriber, object message);
        void Unsubscribe(Address subscriber, object message);
    }
}