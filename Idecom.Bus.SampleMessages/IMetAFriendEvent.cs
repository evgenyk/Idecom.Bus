namespace Idecom.Bus.SampleMessages
{
    using System;

    public interface IMetAFriendEvent
    {
        string Name { get; set; }
        Uri Uri { get; set; }
    }
}