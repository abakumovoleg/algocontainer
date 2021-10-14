using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Algo.Abstracts.Models;

namespace Algo.BackTesting
{
    public class SimpleTestDataProvider : IOrderMatcher, IMarketDepthProvider, Abstracts.Interfaces.IMarketDepthProvider
    {
        private readonly IFileReader _reader;
        private readonly Subject<MarketDepth> _mdSubject = new Subject<MarketDepth>();
        private readonly Subject<Order> _orderSubject = new Subject<Order>();
        private MarketDepth _md;

        public SimpleTestDataProvider(IFileReader reader)
        {
            _reader = reader;
        }

        private readonly SemaphoreSlim _waitOrderSemaphore = new SemaphoreSlim(0, 1);

        private MarketAction _currentAction;
        private readonly AutoResetEvent _nextMarketActionEvent = new AutoResetEvent(false);

        private readonly object _matchLock = new object();
        public async Task Start(string filePath, Security security)
        {
            foreach (var marketAction in _reader.ReadFile(filePath))
            {
                _nextMarketActionEvent.Set();
                _currentAction = marketAction;

                if (marketAction.Type == ActionType.WaitOrder)
                {
                    _nextMarketActionEvent.Reset();

                    await _waitOrderSemaphore.WaitAsync();
                }else if (marketAction.Type == ActionType.Delay)
                {
                    if (marketAction.Delay == null)
                        throw new Exception();

                    await Task.Delay(TimeSpan.FromSeconds(marketAction.Delay.Value));
                }
                else if (marketAction.Type == ActionType.MarketDepth)
                {
                    lock (_matchLock)
                    {
                        _md = marketAction.MarketDepth;
                        _md.Security = security;
                        _mdSubject.OnNext(marketAction.MarketDepth);

                        foreach (var order in _ordersQueue)
                        {
                            Match(order);
                        }
                    }
                }
            }

            _nextMarketActionEvent.Set();
        } 

        private readonly List<Order> _ordersQueue = new List<Order>();

        public void Match(Order order)
        {
            lock (_matchLock)
            {
                var md = _md;

                if (_currentAction.Type == ActionType.WaitOrder)
                {
                    _waitOrderSemaphore.Release(1);
                    _nextMarketActionEvent.WaitOne();
                }

                order.Balance = order.Volume;
                order.OrderState = OrderState.Active;

                decimal executed = 0;

                if (order.Direction == Direction.Buy)
                {
                    foreach (var quote in order.Direction == Direction.Buy ? md.Asks : md.Bids)
                    {
                        if (quote.Volume == 0)
                            continue;

                        if (order.Direction == Direction.Buy
                            ? quote.Price <= order.Price
                            : quote.Price >= order.Price)
                        {
                            executed += Math.Min(order.Balance, quote.Volume);

                            order.Balance -= executed;
                            quote.Volume -= executed;

                            if (order.Balance == 0)
                            {
                                order.OrderState = OrderState.Done;
                                _orderSubject.OnNext(order);
                                break;
                            }

                            _orderSubject.OnNext(order);
                        }
                    }

                    if (order.Balance > 0 && !_ordersQueue.Contains(order))
                        _ordersQueue.Add(order);
                }
            }
        }

        public void Cancel(Order order)
        {
            order.OrderState = OrderState.Done;
            _orderSubject.OnNext(order);
        }

        public IObservable<Order> OrderChanged => _orderSubject;

        public IObservable<MarketDepth> MarketDepthChanged => _mdSubject;

        public void AddSubscription(Security security)
        {
            
        }

        public MarketDepth Get(Security security)
        {
            return _md;
        }
    }
}