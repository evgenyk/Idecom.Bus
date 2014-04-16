using Idecom.Bus.Interfaces;

namespace Idecom.Bus.Implementations
{
    public abstract class Story<TState> : IStory<TState> where TState : IStoryState
    {
        protected IStoryBus Bus { get; private set; }
        public TState StoryState { get; private set; }

        public void CloseStory()
        {
            
        }

    }
}