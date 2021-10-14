using System;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using Algo.Abstracts.Interfaces;
using Algo.Abstracts.Models; 

namespace Algo.Strategies.Execution
{
    public interface IStrategyContext
    {
        IStrategyContext CreateChildContext();
        Task WaitUntilDoneOrCancel(CancellationToken ct);
        void RegisterOrder(Order order);
        void CancelOrder(Order order);
        void SubscribeMarketDepth(Security security, Action<MarketDepth> action);
        void Schedule(TimeSpan delay, Action action, CancellationToken ct); 
        void FailStrategyWhenOrderFailed();
        OrderCollection Orders { get; }
        StrategyLog Log { get; }
        void SetState(StrategyState state);
        StrategyState State { get; }

        void Subscribe(SubscriptionLeaf<Order> subscriptionLeaf);
    }
}