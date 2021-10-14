using System;
using System.Reactive.Disposables;

namespace Algo.Strategies.Execution
{
    public static class DisposableExtensions
    {
        public static void AddToDisposables(this IDisposable disposable, CompositeDisposable container)
        {
            container.Add(disposable);
        }
    }
}