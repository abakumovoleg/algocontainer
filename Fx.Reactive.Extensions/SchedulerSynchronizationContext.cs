using System;
using System.Reactive.Concurrency;
using System.Threading;

namespace Fx.Reactive.Extensions
{
    public class SchedulerSynchronizationContext : SynchronizationContext
    {
        private readonly IScheduler _scheduler;

        public SchedulerSynchronizationContext(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public override void Send(SendOrPostCallback callback, object state)
        {
            throw new NotImplementedException("Too lazy to implemenet synchronous invocation now...");
        }

        public override void Post(SendOrPostCallback callback, object state)
        {
            _scheduler.Schedule(() => callback.Invoke(state));
        }
    }
}