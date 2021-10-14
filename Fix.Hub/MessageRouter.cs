using QuickFix;
using QuickFix.FIXBCS;

namespace Fix.Hub
{
    class MessageRouter
    {
        private readonly MessageHub _messageHub;

        public MessageRouter(MessageHub messageHub)
        {
            _messageHub = messageHub;
        }
        public void Send(SecurityDefinition msg)
        {
            Session.SendToTarget(msg, _messageHub.SecurityDefinitionRequests[msg.SecurityReqID.Obj]);
        }
        
        public void Send(ExecutionReport msg)
        {
            Session.SendToTarget(msg, _messageHub.Orders[msg.ClOrdID.Obj]);
        }

        public void Send(OrderCancelReject msg)
        {
            Session.SendToTarget(msg, _messageHub.OrderCancelRequests[msg.ClOrdID.Obj]);
        }

        public void Send(MarketDataSnapshotFullRefresh msg)
        {
            Session.SendToTarget(msg, _messageHub.MarketDataRequests[msg.MDReqID.Obj]);
        }

        public void Send(MarketDataIncrementalRefresh msg)
        {
            Session.SendToTarget(msg, _messageHub.MarketDataRequests[msg.MDReqID.Obj]);
        }

        public void Send(MarketDataRequestReject msg)
        {
            Session.SendToTarget(msg, _messageHub.MarketDataRequests[msg.MDReqID.Obj]);
        }
    }
}