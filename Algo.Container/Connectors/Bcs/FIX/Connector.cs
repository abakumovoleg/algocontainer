using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Algo.Abstracts.Interfaces;
using Algo.Abstracts.Models;
using Algo.Abstracts.Models.Messages;
using Microsoft.Extensions.Logging;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIXBCS;
using QuickFix.Transport;
using MarketDepth = Algo.Abstracts.Models.MarketDepth;
using Quote = Algo.Abstracts.Models.Quote;
using QuoteType = Algo.Abstracts.Models.QuoteType;
using Subject = QuickFix.Fields.Subject;
using TimeInForce = Algo.Abstracts.Models.TimeInForce;

namespace Algo.Container.Connectors.Bcs.FIX
{
    public class Connector: IConnector
    {
        private readonly IConnectorStorage _storage = new InMemoryStorage();

        public IObservable<Order> OrderChanged { get; private set; }
        public IObservable<Trade> TradeChanged { get; private set; }
        public IObservable<MarketDepth> MarketDepthChanged { get; private set; }
        public IObservable<Connection> ConnectionChanged { get; private set; }
        public IObservable<MarketDataSubscriptionFailMessage> MarketDataSubscriptionFailed { get; private set; }
        public IObservable<RegisterOrderFailMessage> RegisterOrderFailed { get; private set; }
        public IObservable<Security> NewSecurity { get; private set; }

        public IObservable<CancelOrderFailMessage> CancelOrderFailed { get; private set; }


        private readonly Subject<Connection> _connectionSubject = new Subject<Connection>();
        private readonly Subject<Order> _orderSubject = new Subject<Order>();
        private readonly Subject<Trade> _tradeSubject = new Subject<Trade>();
        private readonly Subject<MarketDepth> _marketDepthSubject = new Subject<MarketDepth>();
        private readonly Subject<MarketDataSubscriptionFailMessage> _marketDataSubscriptionFailMessage = new Subject<MarketDataSubscriptionFailMessage>();
        private readonly Subject<RegisterOrderFailMessage> _registerOrderFailMessage = new Subject<RegisterOrderFailMessage>();
        private readonly Subject<CancelOrderFailMessage> _cancelOrderFailMessage = new Subject<CancelOrderFailMessage>();


        private readonly FixServer _fixServer;
        private readonly SocketInitiator _socketInitiator;

        private readonly Connection _marketDataConnection = new Connection{ConnectionType = ConnectionType.MarketData};
        private readonly Connection _transactionConnection = new Connection{ConnectionType =  ConnectionType.Transaction};
        
        private readonly Dictionary<SessionID, Connection> _connections = new Dictionary<SessionID, Connection>();
        private readonly SessionID _transactionSession;
        private readonly SessionID _marketDataSession;
        private readonly ManualResetEvent _transactionConnectionReady = new ManualResetEvent(false);
        private readonly ManualResetEvent _marketDataConnectionReady = new ManualResetEvent(false);

        public Connection[] GetConnections()
        {
            return _connections.Values.ToArray();
        }

        public Connector(string settingsFile, ILoggerFactory loggerFactory)
        {
            var settings = new SessionSettings(settingsFile);
            var sessions = settings.GetSessions();

            _transactionSession = sessions.FirstOrDefault(x => settings.Get(x).GetString("SessionType") == "Transaction");
            _marketDataSession = sessions.FirstOrDefault(x => settings.Get(x).GetString("SessionType") == "MarketData");

            if(_marketDataSession != null)
                _connections[_marketDataSession] = _marketDataConnection;

            if (_transactionSession != null)
                _connections[_transactionSession] = _transactionConnection;

            var storeFactory = new FileStoreFactory(settings);

            var logFactory = new DefaultLogFactory(loggerFactory);

            _fixServer = new FixServer();

            InitializeObservers();
             
            _socketInitiator = new SocketInitiator(
                _fixServer,
                storeFactory,
                settings,
                logFactory);
        }

        private void InitializeObservers()
        {
            ConnectionChanged = _connectionSubject.AsObservable();
            OrderChanged = _orderSubject.AsObservable();
            MarketDepthChanged = _marketDepthSubject.AsObservable();
            TradeChanged = _tradeSubject.AsObservable();
            MarketDataSubscriptionFailed = _marketDataSubscriptionFailMessage.AsObservable();
            RegisterOrderFailed = _registerOrderFailMessage.AsObservable();
            CancelOrderFailed = _cancelOrderFailMessage.AsObservable();

            _fixServer.OnLogOnMsg.Subscribe(OnLogOnMsg);
            _fixServer.OnLogOutMsg.Subscribe(OnLogOutMsg);
            _fixServer.OnMarketDataSnapshotFullRefreshMsg.Subscribe(OnMarketDataSnapshotFullRefreshMsg);
            _fixServer.OnExecutionReportMsg.Subscribe(OnExecutionReport);
            _fixServer.OnTradeCaptureReportMsg.Subscribe(OnTradeCaptureReportMsg);
            _fixServer.OnMarketDataRequestRejectMsg.Subscribe(OnMarketDataRequestRejectMsg);
            _fixServer.OnOrderCancelRejectMsg.Subscribe(OnOrderCancelRejectMsg);

            NewSecurity = _fixServer.OnSecurityDefinitionMsg.SelectMany(OnSecurityDefinition);
        }

        private string GetSecurityKey(string @class, string code)
        {
            return @class + "." + code;
        }

        private IEnumerable<Security> OnSecurityDefinition(SecurityDefinition x)
        {
            var count = x.GroupCount(NoRelatedSym.TAG);

            for (var i = 1; i <= count; i++)
            {
                var grp = (SecurityDefinition.NoRelatedSymGroup) x.GetGroup(i, NoRelatedSym.TAG);
                
                var security = _storage.Securities.Get(GetSecurityKey(grp.UnderlyingSecurityExchange.Obj, grp.UnderlyingSecurityID.Obj)) 
                               ?? new Security(grp.UnderlyingSecurityID.Obj, grp.UnderlyingSecurityExchange.Obj);
                
                security.LotSize = grp.RatioQty.Obj;

                yield return security;
            }
        }

        private void OnOrderCancelRejectMsg(OrderCancelReject x)
        {
            var cancelMessage = (CancelOrderMessage)_storage.Messages.Get(x.ClOrdID.Obj);
            
            _cancelOrderFailMessage.OnNext(new CancelOrderFailMessage
            {
                Order = cancelMessage.Order,
                Error = x.Text.Obj
            });
        }

        private void OnMarketDataRequestRejectMsg(MarketDataRequestReject x)
        {
            var mdsm = (MarketDataSubscriptionMessage) _storage.Messages.Get(x.MDReqID.Obj);

            _marketDataSubscriptionFailMessage.OnNext(new MarketDataSubscriptionFailMessage
            {
                Error = x.Text.Obj,
                MarketDataSubscription = mdsm
            });
        }

        private void OnTradeCaptureReportMsg(TradeCaptureReport x)
        {
            var trade = _storage.Trades.Get(x.ExecID.Obj) ?? new Trade();
            
            var security = _storage.Securities.Get(GetSecurityKey(x.SecurityExchange.Obj, x.SecurityID.Obj)) ?? new Security(x.SecurityID.Obj, x.GetString(ExDestination.TAG));

            var noSidesGroup = (TradeCaptureReport.NoSidesGroup) x.GetGroup(1, NoSides.TAG);

            trade.Id = x.ExecID.Obj;
            trade.Security = security;
            trade.TransactTime = x.TransactTime.Obj;
            trade.Quantity = x.LastQty.Obj;
            trade.Price = x.LastPx.Obj;
            trade.Direction = noSidesGroup.Side.Obj == Side.BUY ? Direction.Buy : Direction.Sell;

            _tradeSubject.OnNext(trade);
        }

        private void OnLogOutMsg(SessionID x)
        {
            var connection = _connections[x];

            connection.ConnectionState = ConnectionState.Disconnected;

            _connectionSubject.OnNext(connection);
        }

        private void OnLogOnMsg(SessionID x)
        {
            var connection = _connections[x];

            connection.ConnectionState = ConnectionState.Connected;
            
            if (connection.ConnectionType == ConnectionType.MarketData)
                _marketDataConnectionReady.Set();

            if (connection.ConnectionType == ConnectionType.Transaction)
                _transactionConnectionReady.Set();

            _connectionSubject.OnNext(connection);
        }

        private void OnMarketDataSnapshotFullRefreshMsg(MarketDataSnapshotFullRefresh x)
        {
            var asks = new List<Quote>();
            var bids = new List<Quote>();

            var @class = x.GetString(ExDestination.TAG);

            var security = _storage.Securities.Get(GetSecurityKey(@class, x.SecurityID.Obj)) ??
                           new Security(x.SecurityID.Obj, @class);

            _storage.Securities.Add(security, security.Code);

            for (var i = 1; i <= x.GroupCount(NoMDEntries.TAG); i++)
            {
                var grp = (MarketDataSnapshotFullRefresh.NoMDEntriesGroup) x.GetGroup(i, NoMDEntries.TAG); 

                var quote = new Quote
                {
                    Security = security,
                    Volume = grp.MDEntrySize.Obj,
                    Price = grp.MDEntryPx.Obj,
                    QuoteType = grp.MDEntryType.Obj == MDEntryType.BID
                        ? QuoteType.Bid
                        : QuoteType.Ask
                };

                if (quote.QuoteType == QuoteType.Ask)
                    asks.Add(quote);
                else
                    bids.Add(quote);
            }

            var md = new MarketDepth(security, asks.ToArray(), bids.ToArray());

            _marketDepthSubject.OnNext(md);
        }



        private void OnExecutionReport(ExecutionReport x)
        {
            var order = _storage.Orders.Get(x.ClOrdID.Obj) ?? _storage.Orders.Get(x.OrigClOrdID.Obj);

            if (order == null)
            {
                var classCode = x.IsSetField(ExDestination.TAG)
                    ? x.GetString(ExDestination.TAG)
                    : x.GetString(SecurityExchange.TAG);

                var security = _storage.Securities.Get(GetSecurityKey(classCode, x.SecurityID.Obj)) ?? new Security(x.SecurityID.Obj, classCode);

                var portfolio = _storage.Portfolios.Get(x.Account.Obj) ?? new Portfolio
                {
                    ClientCode = x.ClientID.Obj,
                    Name = x.Account.Obj
                };

                order = new Order
                {
                    Security = security,
                    Portfolio = portfolio
                };

                _storage.Orders.Add(order, x.ClOrdID.Obj);
            }

            order.OrderId = x.OrderID.Obj;
            order.OrderState = OrderStatusToState(x.OrdStatus.Obj);
            order.Balance = x.OrderQty.Obj - x.CumQty.Obj;// x.LeavesQty.Obj;
            order.OrderState = OrderStatusToState(x.OrdStatus.Obj);
            order.UserOrderId = x.ClOrdID.Obj;
            order.Direction = x.Side.Obj == Side.BUY ? Direction.Buy : Direction.Sell;
            order.Volume = x.OrderQty.Obj;
            order.Price = x.Price.Obj;
            order.OrderType = OrderType.Limit;
            if (x.IsSetField(ClientID.TAG))
                order.ClientCode = x.ClientID.Obj;
            order.TimeInForce = TimeInForce.PutInQueue;
            //order.Time = x.TransactTime.Obj;

            if (x.IsSetText())
                order.Messages.Add(x.Text.Obj);

            _orderSubject.OnNext(order);

            if (x.ExecType.Obj == ExecType.REJECTED)
            {
                _registerOrderFailMessage.OnNext(new RegisterOrderFailMessage
                {
                    Error = x.Text.Obj,
                    Order = order
                });
            }

            if (x.ExecType.Obj == ExecType.FILL ||
                x.ExecType.Obj == ExecType.FILL_OR_PARTIAL_FILL ||
                x.ExecType.Obj == ExecType.PARTIAL_FILL)
            {
                var trade = _storage.Trades.Get(x.ExecID.Obj) ?? new Trade();

                trade.Id = x.ExecID.Obj;
                trade.Security = order.Security;
                trade.TransactTime = x.TransactTime.Obj;
                trade.Quantity = x.LastQty.Obj;
                trade.Price = x.LastPx.Obj;
                trade.Direction = order.Direction;

                _tradeSubject.OnNext(trade);
            }
        }

        public void RequestSecurities()
        {
            Session.SendToTarget(new SecurityDefinitionRequest
            {
                SecurityReqID = new SecurityReqID(Guid.NewGuid().ToString()),
                SecurityRequestType = new SecurityRequestType(SecurityRequestType.REQUEST_LIST_SECURITIES)
            }, _transactionSession);
        }

        public void RegisterOrder(Order order)
        {
            if(order.OrderType != OrderType.Limit && order.OrderType != OrderType.Market)
                throw new Exception($"unsupported OrderType {order.OrderType}");
    
            order.OrderState = OrderState.Pending;

            _storage.Orders.Add(order, order.UserOrderId);

            var newOrder = new NewOrderSingle
            {
                ClOrdID = new ClOrdID(order.UserOrderId),
                Side = new Side(order.Direction == Direction.Buy ? Side.BUY : Side.SELL),
                OrderQty = new OrderQty(order.Volume),
                Price = new Price(order.Price),
                TransactTime = new TransactTime(DateTime.Now),
                OrdType = new OrdType(order.OrderType == OrderType.Market ? OrdType.MARKET : OrdType.LIMIT),
                SecurityID = new SecurityID(order.Security?.Code),
                IDSource = new IDSource(IDSource.EXCHANGE_SYMBOL),
                Account = new Account(order.Portfolio?.Name),
                TimeInForce = new QuickFix.Fields.TimeInForce(order.TimeInForce == TimeInForce.MatchOrCancel
                    ? QuickFix.Fields.TimeInForce.FILL_OR_KILL
                    : QuickFix.Fields.TimeInForce.DAY),
                HandlInst = new HandlInst(HandlInst.AUTOMATED_EXECUTION_ORDER_PRIVATE),
                Symbol = new Symbol(order.Security?.Code),
                ClientID = new ClientID(order.Portfolio?.ClientCode)
            };

            if(order.Comment != null)
                newOrder.Text = new Text(order.Comment);

            Session.SendToTarget(newOrder, _transactionSession); 
        }

        public void CancelOrder(CancelOrderMessage cancelMessage)
        {
            var order = cancelMessage.Order;

            var id = Guid.NewGuid().ToString();

            _storage.Messages.Add(cancelMessage, id);

            var orderCancelRequest = new OrderCancelRequest
            {
                OrigClOrdID = new OrigClOrdID(order.UserOrderId),
                ClOrdID = new ClOrdID(id),
                OrderID = new OrderID(order.OrderId),
                Side = new Side(order.Direction == Direction.Buy ? Side.BUY : Side.SELL),
                OrderQty = new OrderQty(order.Volume)
            };

            Session.SendToTarget(orderCancelRequest, _transactionSession);
        }

        public void SubscribeMarketData(MarketDataSubscriptionMessage mdsm)
        {
            var id = Guid.NewGuid().ToString();

            _storage.Messages.Add(mdsm, id);

            var mdr = new MarketDataRequest
            {
                MDReqID = new MDReqID(id),
                SubscriptionRequestType = new SubscriptionRequestType(SubscriptionRequestType.SNAPSHOT_PLUS_UPDATES),
                MDUpdateType = new MDUpdateType(MDUpdateType.FULL_REFRESH)
            };

            if (mdsm.MarketDataType == MarketDataType.MarketDepth)
            {
                mdr.AddGroup(new MarketDataRequest.NoMDEntryTypesGroup
                {
                    MDEntryType = new MDEntryType(MDEntryType.BID)
                });

                mdr.AddGroup(new MarketDataRequest.NoMDEntryTypesGroup
                {
                    MDEntryType = new MDEntryType(MDEntryType.OFFER)
                });
            }
            else if (mdsm.MarketDataType == MarketDataType.Trades)
            {
                mdr.AddGroup(new MarketDataRequest.NoMDEntryTypesGroup
                {
                    MDEntryType = new MDEntryType(MDEntryType.TRADE)
                });
            }

            var grp = new MarketDataRequest.NoRelatedSymGroup
            {
                Symbol = new Symbol(mdsm.Security.Code),
                IDSource = new IDSource(IDSource.EXCHANGE_SYMBOL),
                SecurityID = new SecurityID(mdsm.Security.Code)
            };

            mdr.SetField(new ExDestination(mdsm.Security.Class));

            mdr.AddGroup(grp); 

            Session.SendToTarget(mdr, _marketDataSession);
        }

        public void Connect()
        {
            foreach (var connection in _connections.Values)
            {
                connection.ConnectionState = ConnectionState.Connecting;
                _connectionSubject.OnNext(connection);
            }

            _socketInitiator.Start();
        }

        public void WaitForConnect()
        {
            WaitHandle.WaitAll(new WaitHandle[]
                {_transactionConnectionReady, _marketDataConnectionReady});
        }

        public void Disconnect()
        {
            foreach (var connection in _connections)
            {
                connection.Value.ConnectionState = ConnectionState.Disconnecting;
                _connectionSubject.OnNext(connection.Value);

                Session.SendToTarget(new Logout(), connection.Key);
            }

            _socketInitiator.Stop();
        }

        private OrderState OrderStatusToState(char status)
        {
            switch (status)
            {
                case OrdStatus.NEW:
                case OrdStatus.PARTIALLY_FILLED:
                    return OrderState.Active;
                case OrdStatus.CANCELED:
                case OrdStatus.REPLACED:
                case OrdStatus.EXPIRED:
                case OrdStatus.FILLED:
                    return OrderState.Done;
                case OrdStatus.REJECTED:
                    return OrderState.Failed;
                case OrdStatus.PENDING_CANCEL:
                case OrdStatus.PENDING_NEW:
                case OrdStatus.PENDING_REPLACE:
                    return OrderState.Pending;

            }

            throw new Exception($"unknown order status {status}");
        }



    }
}
