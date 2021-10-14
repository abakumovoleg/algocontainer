using System;
using System.Collections.Generic;
using Algo.Abstracts.Models;
using Algo.Abstracts.Models.Messages;

namespace Algo.Abstracts.Interfaces
{
    public interface IConnector
    {
        IObservable<Order> OrderChanged { get; }
        IObservable<Trade> TradeChanged { get; }
        IObservable<MarketDepth> MarketDepthChanged { get; }
        IObservable<Connection> ConnectionChanged { get; }
        IObservable<MarketDataSubscriptionFailMessage> MarketDataSubscriptionFailed { get; }
        IObservable<RegisterOrderFailMessage> RegisterOrderFailed { get; }

        IObservable<Security> NewSecurity { get; }

        void RequestSecurities();

        void RegisterOrder(Order order);
        void CancelOrder(CancelOrderMessage cancelMessage);

        void SubscribeMarketData(MarketDataSubscriptionMessage mdsm);

        void Connect();
        void WaitForConnect();
        void Disconnect();

        Connection[] GetConnections();
    }
}