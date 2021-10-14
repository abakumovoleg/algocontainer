using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Algo.Abstracts.Interfaces;
using Algo.Abstracts.Models;
using Algo.Abstracts.Models.Messages;
using Microsoft.Extensions.Logging;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using QuickFix.Transport;
using MarketDepth = Algo.Abstracts.Models.MarketDepth;
using Message = QuickFix.FIX44.Message;
using Quote = Algo.Abstracts.Models.Quote;
using QuoteType = Algo.Abstracts.Models.QuoteType;
using TimeInForce = Algo.Abstracts.Models.TimeInForce;

namespace Algo.Container.Fix.NtPro
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
        public IObservable<MarketDataSubscriptionMessage> MarketDataSubscriptionArrived { get; private set; }


        private readonly Subject<Connection> _connectionSubject = new Subject<Connection>();
        private readonly Subject<Order> _orderSubject = new Subject<Order>();
        private readonly Subject<Trade> _tradeSubject = new Subject<Trade>();
        private readonly Subject<MarketDepth> _marketDepthSubject = new Subject<MarketDepth>();
        private readonly Subject<MarketDataSubscriptionFailMessage> _marketDataSubscriptionFailMessage = new Subject<MarketDataSubscriptionFailMessage>();
        private readonly Subject<RegisterOrderFailMessage> _registerOrderFailMessage = new Subject<RegisterOrderFailMessage>();
        private readonly Subject<CancelOrderFailMessage> _cancelOrderFailMessage = new Subject<CancelOrderFailMessage>();
        private readonly Subject<MarketDataSubscriptionMessage> _markedDataSubscriptionMessage = new Subject<MarketDataSubscriptionMessage>();


        private readonly FixServer _fixServer;
        private readonly SocketInitiator _socketInitiator;

        private readonly Connection _marketDataConnection = new Connection{ConnectionType = ConnectionType.MarketData};
        private readonly Connection _transactionConnection = new Connection{ConnectionType =  ConnectionType.Transaction};
        
        private readonly Dictionary<SessionID, Connection> _connections = new Dictionary<SessionID, Connection>();
        private readonly SessionID _transactionSession;
        private readonly SessionID _marketDataSession;
        private readonly ManualResetEvent _transactionConnectionReady = new ManualResetEvent(false);
        private readonly ManualResetEvent _marketDataConnectionReady = new ManualResetEvent(false);

        private const string SecurityClass = "ntpro";

        public Connection[] GetConnections()
        {
            return _connections.Values.ToArray();
        }

        public Connector(SessionSettings settings, ILoggerFactory loggerFactory)
        {
            var sessions = settings.GetSessions();

            _transactionSession = sessions.FirstOrDefault(x => settings.Get(x).GetString("SessionType") == "Transaction");
            _marketDataSession = sessions.FirstOrDefault(x => settings.Get(x).GetString("SessionType") == "MarketData");

            if (_marketDataSession != null)
                _connections[_marketDataSession] = _marketDataConnection;

            if (_transactionSession != null)
                _connections[_transactionSession] = _transactionConnection;

            var storeFactory = new FileStoreFactory(settings);

            var logFactory = new DefaultLogFactory(loggerFactory);

            _fixServer = new FixServer(settings);

            InitializeObservers();

            _socketInitiator = new SocketInitiator(
                _fixServer,
                storeFactory,
                settings,
                logFactory);
        }

        public Connector(string settingsFile, ILoggerFactory loggerFactory)
            : this(new SessionSettings(settingsFile), loggerFactory)
        {
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
            MarketDataSubscriptionArrived = _markedDataSubscriptionMessage.AsObservable();

            _fixServer.OnLogOnMsg.Subscribe(OnLogOnMsg);
            _fixServer.OnLogOutMsg.Subscribe(OnLogOutMsg);
            _fixServer.OnMarketDataSnapshotFullRefreshMsg.Subscribe(OnMarketDataSnapshotFullRefreshMsg);
            _fixServer.OnMarketDataIncrementalRefreshMsg.Subscribe(OnMarketDataIncrementalRefreshMsg);
            _fixServer.OnExecutionReportMsg.Subscribe(OnExecutionReport);
            _fixServer.OnTradeCaptureReportMsg.Subscribe(OnTradeCaptureReportMsg);
            _fixServer.OnMarketDataRequestRejectMsg.Subscribe(OnMarketDataRequestRejectMsg);
            _fixServer.OnOrderCancelRejectMsg.Subscribe(OnOrderCancelRejectMsg);
            _fixServer.OnMarketDataRequestMsg.Subscribe(OnMarketDataRequestMsg);

            NewSecurity = _fixServer.OnSecurityDefinitionMsg.SelectMany(OnSecurityDefinition);
        }

        private string GetSecurityKey(string code)
        {
            return code;
        }

        private IEnumerable<Security> OnSecurityDefinition(SecurityDefinition x)
        {
            /*
            var count = x.GroupCount(NoRelatedSym.TAG);

            for (var i = 1; i <= count; i++)
            {
                var grp = (SecurityDefinition.NoRelatedSymGroup) x.GetGroup(i, NoRelatedSym.TAG);
                
                var security = _storage.Securities.Get(GetSecurityKey(grp.UnderlyingSecurityExchange.Obj, grp.UnderlyingSecurityID.Obj)) 
                               ?? new Security(grp.UnderlyingSecurityID.Obj, grp.UnderlyingSecurityExchange.Obj);
                
                security.LotSize = grp.RatioQty.Obj;

                yield return security;
            }*/
            throw new NotImplementedException();
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
            
            var security = _storage.Securities.Get(GetSecurityKey(x.SecurityID.Obj)) ?? new Security(x.SecurityID.Obj, SecurityClass);

            var noSidesGroup = (TradeCaptureReport.NoSidesGroup) x.GetGroup(1, x.NoSides.Tag);

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

        private void OnMarketDataIncrementalRefreshMsg(MarketDataIncrementalRefresh x)
        {
            var asks = new List<Quote>();
            var bids = new List<Quote>(); 

            for (var i = 1; i <= x.GroupCount(x.NoMDEntries.Tag); i++)
            {
                var grp = (MarketDataIncrementalRefresh.NoMDEntriesGroup)x.GetGroup(i, x.NoMDEntries.Tag);

                var security = _storage.Securities.Get(GetSecurityKey(grp.Symbol.Obj)) ??
                               new Security(grp.Symbol.Obj, SecurityClass);

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
              
            if (asks.GroupBy(q=>q.Security.Code).Count() > 1)
                throw new Exception();
            if (bids.GroupBy(q => q.Security.Code).Count() > 1)
                throw new Exception();

            if (asks.Count > 0 || bids.Count > 0)
            {
                var md = new MarketDepth((asks.FirstOrDefault() ?? bids.First()).Security, asks.ToArray(),
                    bids.ToArray());

                md.DateTime = GetQuoteDateTime(x);

                _marketDepthSubject.OnNext(md);
            }
        }

        private static DateTime? GetQuoteDateTime(Message x)
        {
            var format = "yyyyMMdd-HH:mm:ss.fffffff";

            if (x.IsSetField(10011))
                return DateTime.ParseExact(x.GetString(10011).Substring(0, format.Length), format,
                    CultureInfo.InvariantCulture);

            return null;
        }

        private void OnMarketDataSnapshotFullRefreshMsg(MarketDataSnapshotFullRefresh x)
        {
            var asks = new List<Quote>();
            var bids = new List<Quote>();

            var security = _storage.Securities.Get(GetSecurityKey(x.Symbol.Obj)) ??
                           new Security(x.Symbol.Obj, SecurityClass);

            _storage.Securities.Add(security, security.Code);

            for (var i = 1; i <= x.GroupCount(x.NoMDEntries.Tag); i++)
            {
                var grp = (MarketDataSnapshotFullRefresh.NoMDEntriesGroup) x.GetGroup(i, x.NoMDEntries.Tag); 

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

            md.DateTime = GetQuoteDateTime(x);

            _marketDepthSubject.OnNext(md);
        }



        private void OnExecutionReport(ExecutionReport x)
        {
            var order = _storage.Orders.Get(x.ClOrdID.Obj) ?? _storage.Orders.Get(x.OrigClOrdID.Obj);

            if (order == null)
            {
                //var classCode = x.IsSetField(ExDestination.TAG) ? x.GetString(ExDestination.TAG) : x.GetString(SecurityExchange.TAG);

                var security = _storage.Securities.Get(GetSecurityKey(x.SecurityID.Obj)) ?? new Security(x.SecurityID.Obj, SecurityClass);

                var portfolio = _storage.Portfolios.Get(x.Account.Obj) ?? new Portfolio
                {
                    //ClientCode = x.ClientID.Obj,
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
            //if (x.IsSetField(ClientID.TAG))
            //    order.ClientCode = x.ClientID.Obj;
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
                //IDSource = new IDSource(IDSource.EXCHANGE_SYMBOL),
                Account = new Account(order.Portfolio?.Name),
                TimeInForce = new QuickFix.Fields.TimeInForce(order.TimeInForce == TimeInForce.MatchOrCancel
                    ? QuickFix.Fields.TimeInForce.FILL_OR_KILL
                    : QuickFix.Fields.TimeInForce.DAY),
                HandlInst = new HandlInst(HandlInst.AUTOMATED_EXECUTION_ORDER_PRIVATE),
                Symbol = new Symbol(order.Security?.Code),
                //ClientID = new ClientID(order.Portfolio?.ClientCode)
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
                MDUpdateType = new MDUpdateType(mdsm.MarketDataSubscriptionType == MarketDataSubscriptionType.Increment ? MDUpdateType.INCREMENTAL_REFRESH : MDUpdateType.FULL_REFRESH),
                NoRelatedSym = new NoRelatedSym(1),
                NoMDEntryTypes = new NoMDEntryTypes(2),
                MarketDepth = new QuickFix.Fields.MarketDepth(0)
            };

            if (mdsm.MarketDataSubscriptionQuoteType == MarketDataSubscriptionQuoteType.Order)
                mdr.SetField(new IntField(10012, 1));
            else if (mdsm.MarketDataSubscriptionQuoteType == MarketDataSubscriptionQuoteType.Band)
                mdr.SetField(new IntField(10012, 2));

            mdr.SetField(new StringField(10010, "Y"));

            mdr.AddGroup(new MarketDataRequest.NoRelatedSymGroup
            {
                Symbol = new Symbol(mdsm.Security.Code)
            });

            mdr.AddGroup(new MarketDataRequest.NoMDEntryTypesGroup
            {
                MDEntryType = new MDEntryType(MDEntryType.BID)
            });

            mdr.AddGroup(new MarketDataRequest.NoMDEntryTypesGroup
            {
                MDEntryType = new MDEntryType(MDEntryType.OFFER)
            });

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
            if (_transactionSession != null)
                _transactionConnectionReady.WaitOne();

            if (_marketDataSession != null)
                _marketDataConnectionReady.WaitOne();
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

        public void PublishMarketDepth(MarketDepth md, string requestId)
        {
            var msg = new MarketDataSnapshotFullRefresh
            {
                MDReqID = new MDReqID(requestId),
                Symbol = new Symbol(md.Security.Code),
                NoMDEntries = new NoMDEntries(md.Bids.Length + md.Asks.Length)
            };
            var quoteId = 0;

            foreach (var quote in md.Bids)
            {
                var bidGroup = new MarketDataSnapshotFullRefresh.NoMDEntriesGroup
                {
                    QuoteEntryID = new QuoteEntryID(quoteId.ToString()),
                    MinQty = new MinQty(1),
                    MDEntryType = new MDEntryType(MDEntryType.BID),
                    MDEntryPx = new MDEntryPx(quote.Price),
                    MDEntrySize = new MDEntrySize(quote.Volume)
                };

                quoteId++;

                msg.AddGroup(bidGroup);
            }

            foreach (var quote in md.Asks)
            {
                var askGroup = new MarketDataSnapshotFullRefresh.NoMDEntriesGroup
                {
                    QuoteEntryID = new QuoteEntryID(quoteId.ToString()),
                    MinQty = new MinQty(1),
                    MDEntryType = new MDEntryType(MDEntryType.OFFER),
                    MDEntryPx = new MDEntryPx(quote.Price),
                    MDEntrySize = new MDEntrySize(quote.Volume)
                };

                quoteId++;

                msg.AddGroup(askGroup);
            }

            Session.SendToTarget(msg, _marketDataSession);
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

        private void OnMarketDataRequestMsg(MarketDataRequest x)
        {
            var symbol = x.GetGroup(1, Tags.NoRelatedSym).GetField(Tags.Symbol);

            _markedDataSubscriptionMessage.OnNext(new MarketDataSubscriptionMessage
            {
                MarketDataSubscriptionType = MarketDataSubscriptionType.Snapshot,
                MarketDataType = MarketDataType.MarketDepth,
                Security = new Security(symbol, "ntpro"),
                MessageId = x.MDReqID.Obj
            });
        }
    }
}
