using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Algo.Abstracts.Interfaces;
using Algo.Abstracts.Models;
using Algo.Abstracts.Models.Messages;
using Algo.Strategies.Execution.GentleExecution;
using Microsoft.Extensions.Logging;

namespace Algo.Strategies.Execution
{
    public class StrategyContext : IStrategyContext
    { 
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0, 1);

        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        public StrategyContext(IConnector connector, IMarketDepthProvider marketDepthProvider, IScheduler scheduler, ILogger logger)
        { 
            Connector = connector;
            MarketDepthProvider = marketDepthProvider;
            Scheduler = scheduler;
            Log = new StrategyLog(logger);
        }

        public StrategyContext(IConnector connector, IMarketDepthProvider marketDepthProvider, IScheduler scheduler, StrategyLog log)
        {
            Connector = connector;
            MarketDepthProvider = marketDepthProvider;
            Scheduler = scheduler;
            Log = log;
        }

        public IConnector Connector { get; }
        public IMarketDepthProvider MarketDepthProvider { get; }
        public IScheduler Scheduler { get; }

        public void Schedule(TimeSpan delay, Action action, CancellationToken ct)
        {
            var disposable = Scheduler.Schedule(delay, action);

            disposable.AddToDisposables(_disposables);

            ct.Register(disposable.Dispose);
        }

        public void RegisterOrder(Order order)
        {
            Log.LogInfo($"RegisterOrder {order}");

            Orders.Add(order);
            Connector.RegisterOrder(order);
        } 

        public void SubscribeMarketDepth(Security security, Action<MarketDepth> action)
        {
            if(security == null)
                throw new Exception("security is null");

            Connector.MarketDepthChanged.Where(x => x?.Security != null && x.Security.Equals(security))
                .Subscribe(action).AddToDisposables(_disposables);

            action(MarketDepthProvider.Get(security));
        }

        public void FailStrategyWhenOrderFailed()
        {
            Connector.OrderChanged
                .Where(x => x.OrderState == OrderState.Failed && Orders.Contains(x))
                .ObserveOn(Scheduler)
                .Subscribe(x => SetState(StrategyState.Failed))
                .AddToDisposables(_disposables);
        }

        public void CancelOrder(Order order)
        {
            Log.LogInfo($"CancelOrder {order}");

            Connector.CancelOrder(new CancelOrderMessage(order));
        } 

        public OrderCollection Orders { get; } = new OrderCollection();

        public StrategyLog Log { get; }

        public StrategyState State { get; private set; } = StrategyState.Init;

        public void SetState(StrategyState newState)
        {
            if (newState == State)
                return;

            Log.LogInfo($"OldState = {State}, NewState = {newState}");

            State = newState;

            if (!State.IsFinal())
                return;

            _disposables.Dispose();

            if (_semaphore.CurrentCount == 0)
                _semaphore.Release(1); 
        }

        public IStrategyContext CreateChildContext()
        {
            return new StrategyContext(Connector, MarketDepthProvider, Scheduler, Log);
        }

        public async Task WaitUntilDoneOrCancel(CancellationToken ct)
        {
            try
            {
                await _semaphore.WaitAsync(ct);
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                try
                {
                    Connector.CancelAllActiveOrders(Orders);
                }
                catch(Exception)
                {
                    // ignored
                }
                finally
                {
                    _disposables.Dispose(); 
                }
            }
        }

        public void Subscribe(SubscriptionLeaf<Order> subscriptionLeaf)
        {
            var disposable = new SingleAssignmentDisposable();

            disposable.Disposable = subscriptionLeaf.SubscriptionNode.Func(Connector.OrderChanged)
                .ObserveOn(Scheduler)
                .Subscribe(x =>
                { 
                    subscriptionLeaf.Action(x);
                    if (subscriptionLeaf.Mode == SubscriptionLeafMode.Once)
                    {
                        disposable.Dispose();
                    }
                });

            disposable.AddToDisposables(_disposables);
        }

    }
}