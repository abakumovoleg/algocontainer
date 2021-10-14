using System;
using System.Reactive.Subjects;
using Algo.Abstracts.Interfaces;
using Algo.Abstracts.Models;
using Algo.Abstracts.Models.Messages;

namespace Algo.BackTesting
{
    public class TradingSystemEmulator : IConnector
    {
        private readonly IOrderMatcher _orderMatcher;
        private readonly IMarketDepthProvider _mdp;
        private readonly Connection _mdConnection = new Connection();
        private readonly Connection _tsConnection = new Connection();

        private readonly Subject<Connection> _connectionSubject = new Subject<Connection>();  

        public TradingSystemEmulator(IOrderMatcher orderMatcher, IMarketDepthProvider mdp)
        {
            _orderMatcher = orderMatcher;
            _mdp = mdp;
            OrderChanged = _orderMatcher.OrderChanged;
            
        }

        public IObservable<Order> OrderChanged {get; }
        public IObservable<Trade> TradeChanged { get; } = new Subject<Trade>();
        public IObservable<MarketDepth> MarketDepthChanged => _mdp.MarketDepthChanged;
        public IObservable<Connection> ConnectionChanged => _connectionSubject;
        public IObservable<MarketDataSubscriptionFailMessage> MarketDataSubscriptionFailed { get; } = new Subject<MarketDataSubscriptionFailMessage>();
        public IObservable<RegisterOrderFailMessage> RegisterOrderFailed { get; } = new Subject<RegisterOrderFailMessage>();

        public IObservable<Security> NewSecurity => new Subject<Security>();

        public void RegisterOrder(Order order)
        {
            _orderMatcher.Match(order);
        }

        public void CancelOrder(CancelOrderMessage cancelMessage)
        {
            _orderMatcher.Cancel(cancelMessage.Order);
        }

        public void SubscribeMarketData(MarketDataSubscriptionMessage mdsm)
        {
        }

        public void Connect()
        {
            _tsConnection.ConnectionState = ConnectionState.Connected;
            _mdConnection.ConnectionState = ConnectionState.Connected;

            _connectionSubject.OnNext(_tsConnection);
            _connectionSubject.OnNext(_mdConnection);
        }

        public void WaitForConnect()
        {
            // ignore
        }

        public void Disconnect()
        {
            _tsConnection.ConnectionState = ConnectionState.Disconnected;
            _mdConnection.ConnectionState = ConnectionState.Disconnected;

            _connectionSubject.OnNext(_tsConnection);
            _connectionSubject.OnNext(_mdConnection);
        }

        public Connection[] GetConnections()
        {
            return new[] {_mdConnection, _tsConnection};
        }

        public void RequestSecurities()
        { 
        }
    }
}