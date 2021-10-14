using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Algo.Abstracts.Interfaces;
using Algo.Abstracts.Models;
using Algo.Abstracts.Models.Messages;
using Microsoft.Extensions.Logging;

namespace Algo.Container
{
    public class MarketDepthProvider : IMarketDepthProvider
    {
        private readonly IConnector _connector;

        private readonly ConcurrentDictionary<Security, MarketDepth> _marketDepths =
            new ConcurrentDictionary<Security, MarketDepth>();

        private readonly ConcurrentDictionary<Security, MarketDepthSubscription> _subscriptions = new ConcurrentDictionary<Security, MarketDepthSubscription>();

        private readonly object _syncObject = new object();  

        public MarketDepthProvider(IConnector connector, ILogger<MarketDepthProvider> logger)
        {
            _connector = connector;

            connector.ConnectionChanged
                .Where(x => x.ConnectionType == ConnectionType.MarketData) 
                .Subscribe(x =>
                {
                    lock (_syncObject)
                    {
                        if (x.ConnectionState == ConnectionState.Connected)
                        {
                            SubscribeToMarketDepths();
                        }
                        else
                        {
                            _marketDepths.Clear();

                            foreach (var marketDepthSubscription in _subscriptions.Values)
                            {
                                marketDepthSubscription.Subscribed = false;
                            }
                        }
                    }
                });

            connector.MarketDepthChanged
                .Where(x => x.Security != null) 
                .Subscribe(x =>
                {
                    if (_subscriptions.TryGetValue(x.Security, out var mds))
                        mds.Subscribed = true;

                    _marketDepths[x.Security] = x;
                });

            connector.MarketDataSubscriptionFailed 
                .Subscribe(x =>
                {
                    if(_subscriptions.TryGetValue(x.MarketDataSubscription.Security, out var mds))
                    {
                        mds.Subscribed = false;

                        Task.Run(async () =>
                        {
                            try
                            {
                                await Task.Delay(60 * 1000);
                                SubscribeToMarketDepth(mds);
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, ex.Message);
                            }
                        });
                    }

                    logger.LogError($"MarketDataSubscription failed {x.MarketDataSubscription} {x.Error}, retry after 1 minute");
                });
        }

        private void SubscribeToMarketDepths()
        {
            foreach (var subscription in _subscriptions)
            {
                SubscribeToMarketDepth(subscription.Value);
            }
        }

        private void SubscribeToMarketDepth(MarketDepthSubscription subscription)
        {
            lock (_syncObject)
            {
                _connector.SubscribeMarketData(new MarketDataSubscriptionMessage
                {
                    MarketDataType = MarketDataType.MarketDepth,
                    Security = subscription.Security
                });
            }
        }

   
        public void AddSubscription(Security security)
        {
            lock (_syncObject)
            {
                if (_subscriptions.Keys.Contains(security))
                    return;
                
                var subscription = new MarketDepthSubscription(security);

                _subscriptions[security] = subscription;

                if (_connector.GetConnections().Any(x =>
                    x.ConnectionType == ConnectionType.MarketData && x.ConnectionState == ConnectionState.Connected))
                {
                    SubscribeToMarketDepth(subscription);
                }
            }
        }

        public MarketDepth Get(Security security)
        {
            if (_marketDepths.TryGetValue(security, out var md))
                return md;

            return null;
        }
    }
}