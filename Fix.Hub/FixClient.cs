using QuickFix;
using QuickFix.FIXBCS;
using Message = QuickFix.Message;

namespace Fix.Hub
{
    class FixClient : MessageCracker, IApplication
    {
        private readonly MessageRouter _messageRouter;
        
        public FixClient(MessageRouter messageRouter)
        {
            _messageRouter = messageRouter;
        }
        public void ToAdmin(Message message, SessionID sessionID)
        {
            //throw new NotImplementedException();
        }

        public void FromAdmin(Message message, SessionID sessionID)
        {
            //throw new NotImplementedException();
        }

        public void ToApp(Message message, SessionID sessionID)
        {
            //throw new NotImplementedException();
        }

        public void FromApp(Message message, SessionID sessionID)
        {
            Crack(message, sessionID);
        }

        public void OnCreate(SessionID sessionID)
        {
            //throw new NotImplementedException();
        }

        public void OnLogout(SessionID sessionID)
        {
            //throw new NotImplementedException();
        }

        public void OnLogon(SessionID sessionID)
        {
            //throw new NotImplementedException();
        }

        public void OnMessage(SecurityDefinition message, SessionID sessionId)
        {
            _messageRouter.Send(message);
        }
        
        public void OnMessage(ExecutionReport message, SessionID sessionId)
        {
            _messageRouter.Send(message);
        }

        public void OnMessage(OrderCancelReject message, SessionID sessionId)
        {
            _messageRouter.Send(message);
        }

        public void OnMessage(MarketDataSnapshotFullRefresh message, SessionID sessionId)
        {
            _messageRouter.Send(message);
        }

        public void OnMessage(MarketDataRequestReject message, SessionID sessionId)
        {
            _messageRouter.Send(message);
        }
    }
}