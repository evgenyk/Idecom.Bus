﻿namespace Idecom.Bus.Interfaces.Addons.PubSub
{
    using System;
    using System.Collections.Generic;
    using Addressing;

    public interface ISubscriptionStorage
    {
        IEnumerable<Address> GetSubscribersFor<T>() where T : class;
        void Subscribe(Address subscriber, Type type);
        void Unsubscribe<T>(Address subscriber) where T : class;
    }

    public interface ISagaStorage
    {

    }
}