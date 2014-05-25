﻿using Idecom.Bus.Interfaces.Addons.Stories;

namespace Idecom.Bus.SampleApp1.Bus1Handlers
{
    public class NicePersonConversationState : ISagaState
    {
        public bool FriendSaidHello { get; set; }
        public bool RepliedBackWithHello { get; set; }

    }
}