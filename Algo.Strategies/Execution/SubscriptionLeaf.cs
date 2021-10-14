using System;

namespace Algo.Strategies.Execution
{
    public class SubscriptionLeaf<T>
    {
        public SubscriptionNode<T> SubscriptionNode { get; }

        public Action<T> Action { get; }
        public SubscriptionLeafMode Mode { get; }

        public SubscriptionLeaf(SubscriptionNode<T> subscriptionNode, Action<T> action, SubscriptionLeafMode mode)
        {
            SubscriptionNode = subscriptionNode;
            Action = action;
            Mode = mode;
        }


    }
}