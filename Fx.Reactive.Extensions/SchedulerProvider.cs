using System.Reactive.Concurrency;
using System.Threading;

namespace Fx.Reactive.Extensions
{
    public class SchedulerProvider
    {
        public SchedulerProvider()
        {
            Scheduler = new EventLoopScheduler();
            Scheduler.Schedule(() =>
            {
                var syncContext = new SchedulerSynchronizationContext(Scheduler);
                SynchronizationContext.SetSynchronizationContext(syncContext);
            });
        }

        public EventLoopScheduler Scheduler { get; }
    }
}
