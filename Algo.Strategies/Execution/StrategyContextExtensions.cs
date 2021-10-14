using System;
using Algo.Abstracts.Models;

namespace Algo.Strategies.Execution
{
    public static class StrategyContextExtensions
    {
        public static void Subscribe(this SubscriptionLeaf<Order> subscription, IStrategyContext context)
        {
            context.Subscribe(subscription);
        }

        public static void Finish(this IStrategyContext context, decimal volume)
        {
            context.SetState(context.Orders.Executed() >= volume ? StrategyState.Completed : StrategyState.Failed);
        }
    }
}