using Idecom.Bus.Interfaces;

namespace Idecom.Bus.Implementations
{
    public abstract class Story<TState> : IStory<TState> where TState : IStoryState
    {
        public IBus Bus { get; set; }
        public TState StoryState { get; set; }

        public void CloseStory()
        {
            
        }

    }
}