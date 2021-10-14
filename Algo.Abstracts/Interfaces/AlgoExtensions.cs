using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Algo.Abstracts.Models;

namespace Algo.Abstracts.Interfaces
{
    public static class AlgoExtensions
    {
        public static IObservable<Order> WhenDone(this Order order, IConnector connector)
        {
            return connector.OrderChanged
                .Where(x => x == order && x.OrderState == OrderState.Done)
                .FirstOrDefaultAsync();
        }

        public static IObservable<Order> WhenFail(this Order order, IConnector connector)
        {
            return connector.OrderChanged
                .Where(x => x == order && x.OrderState == OrderState.Done)
                .FirstOrDefaultAsync();
        }

        public static IDisposable ThenOnce<T>(this IObservable<T> observable, Action<T> action, IScheduler scheduler)
        {
            var single = new SingleAssignmentDisposable();

            single.Disposable = observable.SubscribeOn(scheduler).Subscribe(x =>
            {
                action(x);

                single.Dispose();
            });

            return single;
        }
    }
}