using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using Algo.Abstracts.Interfaces;

namespace Algo.Strategies.Execution
{ 
    public class SubscriptionNode<T>
    {
        public Func<IObservable<T>, IObservable<T>> Func { get; }

        public SubscriptionNode(Func<IObservable<T>, IObservable<T>> func)
        {
            Func = func;
        }
    }
}