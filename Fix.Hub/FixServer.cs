using QuickFix;
using QuickFix.FIXBCS;
using Message = QuickFix.Message;

namespace Fix.Hub
{
    class FixServer : MessageCracker, IApplication
    {
        private readonly MessageProxy _messageProxy;

        public FixServer(MessageProxy messageProxy)
        {
            _messageProxy = messageProxy;
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

        public void OnMessage(SecurityDefinitionRequest message, SessionID sessionId)
        {
            _messageProxy.Send(message, sessionId);
        }

        public void OnMessage(NewOrderSingle message, SessionID sessionId)
        {
            _messageProxy.Send(message, sessionId);
        }

        public void OnMessage(OrderCancelRequest message, SessionID sessionId)
        {
            _messageProxy.Send(message, sessionId);
        }

        public void OnMessage(MarketDataRequest message, SessionID sessionId)
        {
            _messageProxy.Send(message, sessionId);
        }
    }
}