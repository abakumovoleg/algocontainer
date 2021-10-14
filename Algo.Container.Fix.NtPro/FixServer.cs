using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using Message = QuickFix.FIX44.Message;

namespace Algo.Container.Fix.NtPro
{
    public class FixServer : MessageCracker, IApplication
    {
        private readonly SessionSettings _settings;
        private readonly Subject<SessionID> _logon = new Subject<SessionID>();
        private readonly Subject<SessionID> _logout = new Subject<SessionID>();
        private readonly Subject<ExecutionReport> _executionReport = new Subject<ExecutionReport>();
        private readonly Subject<TradeCaptureReportAck> _tradeCaptureReportAck = new Subject<TradeCaptureReportAck>();
        private readonly Subject<TradeCaptureReport> _tradeCaptureReport = new Subject<TradeCaptureReport>();
        private readonly Subject<MarketDataSnapshotFullRefresh> _marketDataSnapshotFullRefresh = new Subject<MarketDataSnapshotFullRefresh>();
        private readonly Subject<MarketDataIncrementalRefresh> _marketDataIncrementalRefresh = new Subject<MarketDataIncrementalRefresh>();
        private readonly Subject<MarketDataRequestReject> _marketDataRequestReject = new Subject<MarketDataRequestReject>();
        private readonly Subject<OrderCancelReject> _orderCancelReject  = new Subject<OrderCancelReject>();
        private readonly Subject<SecurityDefinition> _securityDefinition = new Subject<SecurityDefinition>();
        private readonly Subject<MarketDataRequest> _marketDataRequest = new Subject<MarketDataRequest>();

        private SessionID _sessionId;

        public IObservable<SessionID> OnLogOnMsg { get; }
        public IObservable<SessionID> OnLogOutMsg { get; }

        public IObservable<ExecutionReport> OnExecutionReportMsg { get;  }
        public IObservable<MarketDataSnapshotFullRefresh> OnMarketDataSnapshotFullRefreshMsg { get; }
        public IObservable<MarketDataIncrementalRefresh> OnMarketDataIncrementalRefreshMsg { get; }
        public IObservable<TradeCaptureReportAck> OnTradeCaptureReportAckMsg { get; }
        public IObservable<TradeCaptureReport> OnTradeCaptureReportMsg { get; } 
        public IObservable<MarketDataRequestReject> OnMarketDataRequestRejectMsg { get; }
        public IObservable<OrderCancelReject> OnOrderCancelRejectMsg { get; }
        public IObservable<SecurityDefinition> OnSecurityDefinitionMsg { get; }

        public IObservable<MarketDataRequest> OnMarketDataRequestMsg { get; }

        public FixServer(SessionSettings settings)
        {
            _settings = settings;
            OnLogOnMsg = _logon.AsObservable();
            OnLogOutMsg = _logout.AsObservable();
            OnExecutionReportMsg = _executionReport.AsObservable();
            OnMarketDataSnapshotFullRefreshMsg = _marketDataSnapshotFullRefresh.AsObservable();
            OnTradeCaptureReportAckMsg = _tradeCaptureReportAck.AsObservable();
            OnTradeCaptureReportMsg = _tradeCaptureReport.AsObservable();
            OnMarketDataIncrementalRefreshMsg = _marketDataIncrementalRefresh.AsObservable();
            OnMarketDataRequestRejectMsg = _marketDataRequestReject.AsObservable();
            OnOrderCancelRejectMsg = _orderCancelReject.AsObservable();
            OnSecurityDefinitionMsg = _securityDefinition.AsObservable();
            OnMarketDataRequestMsg = _marketDataRequest.AsObservable();
        }
         
          
        public void OnCreate(SessionID sessionId)
        {
            //Console.WriteLine("OnCreate");
        }

        public void OnLogout(SessionID sessionId)
        {
            _logout.OnNext(sessionId);
        }

        public void OnMessage(SecurityDefinition message, SessionID sessionId)
        {
            _securityDefinition.OnNext(message);
        }

         
#pragma warning disable IDE0060 // Remove unused parameter
        public void OnMessage(ExecutionReport message, SessionID sessionId)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            _executionReport.OnNext(message);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public void OnMessage(MarketDataSnapshotFullRefresh message, SessionID sessionId)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            _marketDataSnapshotFullRefresh.OnNext(message);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public void OnMessage(OrderCancelReject message, SessionID sessionId)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            _orderCancelReject.OnNext(message);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public void OnMessage(MarketDataIncrementalRefresh message, SessionID sessionId)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            //var grp = (MarketDataIncrementalRefresh.NoMDEntriesGroup) message.GetGroup(1,
            //    new MarketDataIncrementalRefresh.NoMDEntriesGroup());
            //var t = grp.TradingSessionID.Obj;

            _marketDataIncrementalRefresh.OnNext(message);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public void OnMessage(TradeCaptureReportAck message, SessionID sessionId)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            _tradeCaptureReportAck.OnNext(message);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public void OnMessage(TradeCaptureReport message, SessionID sessionId)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            _tradeCaptureReport.OnNext(message);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public void OnMessage(MarketDataRequestReject message, SessionID sessionId)
#pragma warning restore IDE0060 // Remove unused parameter
        {
             _marketDataRequestReject.OnNext(message);
        }


        public void SendQuoteRequest(QuoteRequest quoteRequest)
        {
            Session.SendToTarget(quoteRequest, _sessionId);
        }
        
        public void OnLogon(SessionID sessionId)
        { 
            _sessionId = sessionId; 
            _logon.OnNext(sessionId);
            //Session.SendToTarget(new MarketDataRequest(new MDReqID(), new SubscriptionRequestType('0'), new MarketDepth(0)), sessionId);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public void FromAdmin(Message msg, SessionID sessionId)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            Console.WriteLine($"FromAdmin:{msg}");
        }
 

#pragma warning disable IDE0060 // Remove unused parameter
        public void ToApp(Message msg, SessionID sessionId)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            Console.WriteLine("OUT:  " + msg);
        }

        public void ToAdmin(QuickFix.Message message, SessionID sessionID)
        {
            if (message.Header.GetString(35) == MsgType.LOGON)
            {
                var session = _settings.Get(sessionID);
                if(!session.Has("Password"))
                    return;

                var password = _settings.Get(sessionID).GetString("Password");
                message.SetField(new Password(password));
            }
        }

        public void FromAdmin(QuickFix.Message message, SessionID sessionID)
        {
            //throw new NotImplementedException();
        }

        public void ToApp(QuickFix.Message message, SessionID sessionID)
        {
            //throw new NotImplementedException();
        }

        public void FromApp(QuickFix.Message message, SessionID sessionID)
        {
            Crack(message, sessionID);
        }

        public void OnMessage(TradingSessionStatus message, SessionID sessionId)
        {

        }
        public void OnMessage(MarketDataRequest message, SessionID sessionId)
        {
            _marketDataRequest.OnNext(message);
        }
    }
}