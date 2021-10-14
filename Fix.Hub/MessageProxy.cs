using QuickFix;
using QuickFix.FIXBCS;

namespace Fix.Hub
{
    class MessageProxy
    {
        private readonly MessageHub _messageHub;
        private readonly SessionID _session;
        
        public MessageProxy(MessageHub messageHub, SessionID session)
        {
            _messageHub = messageHub;
            _session = session;
        }

        public void Send(SecurityDefinitionRequest msg, SessionID sessionID)
        {
            _messageHub.SecurityDefinitionRequests[msg.SecurityReqID.Obj] = sessionID;
            
            Session.SendToTarget(msg, _session);
        }

        public void Send(NewOrderSingle msg, SessionID sessionID)
        {  
            _messageHub.Orders[msg.ClOrdID.Obj] = sessionID;

            Session.SendToTarget(msg, _session);
        }
        
        public void Send(OrderCancelRequest msg, SessionID sessionID)
        {
            _messageHub.OrderCancelRequests[msg.ClOrdID.Obj] = sessionID;

            Session.SendToTarget(msg, _session);
        }

        public void Send(MarketDataRequest msg, SessionID sessionID)
        {
            _messageHub.MarketDataRequests[msg.MDReqID.Obj] = sessionID;

            Session.SendToTarget(msg, _session);
        }

    }
}