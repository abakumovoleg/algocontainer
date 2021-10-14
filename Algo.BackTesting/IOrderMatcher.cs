using System;
using Algo.Abstracts.Models;

namespace Algo.BackTesting
{
    public interface IOrderMatcher
    { 
        void Match(Order order);
        void Cancel(Order order);
        IObservable<Order> OrderChanged { get; }
    }
}