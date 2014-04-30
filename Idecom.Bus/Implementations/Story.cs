namespace Idecom.Bus.Implementations
{
    using Interfaces;
    using Interfaces.Addons.Stories;

    public abstract class Story<TState> : IStory<TState> where TState : IStoryState
    {
        public IBus Bus { get; set; }
        public TState StoryState { get; set; }

        public void CloseStory()
        {
        }
    }
}