namespace Idecom.Bus.Interfaces.Addons.Stories
{
    using System;

    public interface IStory<out TState> where TState : IStoryState
    {
        TState StoryState { get; }
    }

    public abstract class StoryState : IStoryState
    {
        private Guid _id;

        protected StoryState()
        {
            _id = Guid.NewGuid();
        }
    }
}