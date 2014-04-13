using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Idecom.Bus.Transport.MongoDB.MongoDbTransport
{
    internal class MaxWorkersTaskScheduler : TaskScheduler, IDisposable
    {
        private readonly List<Thread> _threads;
        private bool _disposed;
        private BlockingCollection<Task> _tasks;

        public MaxWorkersTaskScheduler(int workersCount)
        {
            _tasks = new BlockingCollection<Task>();

            _threads = Enumerable.Range(0, workersCount).Select(i =>
            {
                var thread = new Thread(() =>
                {
                    foreach (Task t in _tasks.GetConsumingEnumerable())
                        TryExecuteTask(t);
                }) {IsBackground = true};

                thread.SetApartmentState(ApartmentState.MTA);
                thread.Name = String.Format("ReceiverThread - {0}", thread.ManagedThreadId);
                return thread;
            }).ToList();
            _threads.ForEach(t => t.Start());
        }

        public int TasksPending
        {
            get { return _tasks.Count(); }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (_tasks == null) return;

            _tasks.CompleteAdding();
            foreach (Thread thread in _threads)
                thread.Join();
            _tasks.Dispose();
            _tasks = null;
        }

        protected override void QueueTask(Task task)
        {
            _tasks.Add(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _tasks.ToArray();
        }
    }
}