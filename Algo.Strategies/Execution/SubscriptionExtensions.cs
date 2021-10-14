using System;
using System.Reactive.Linq;
using Algo.Abstracts.Models;

namespace Algo.Strategies.Execution
{
    public static class SubscriptionExtensions
    {
        public static SubscriptionNode<Order> WhenDone(this Order order)
        {
            return new SubscriptionNode<Order>(o =>
                o.Where(x => x == order && x.OrderState == OrderState.Done));
        }

        public static SubscriptionLeaf<T> Then<T>(this SubscriptionNode<T> subscriptionNode, Action<T> action)
        {
            return new SubscriptionLeaf<T>(subscriptionNode, action, SubscriptionLeafMode.None);
        }

        public static SubscriptionLeaf<T> ThenOnce<T>(this SubscriptionNode<T> subscriptionNode, Action<T> action)
        {
            return new SubscriptionLeaf<T>(subscriptionNode, action, SubscriptionLeafMode.Once);
        }
    }
}