using System;

namespace Idecom.Bus.SampleMessages
{
    public interface IMetAFriendEvent
    {
        string Name { get; set; }
        Uri Uri { get; set; }
    }
}