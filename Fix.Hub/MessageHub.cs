using System.Collections.Generic;
using QuickFix;

namespace Fix.Hub
{
    public class MessageHub
    {
        public Dictionary<string, SessionID> SecurityDefinitionRequests { get; } = new Dictionary<string, SessionID>();
        public Dictionary<string, SessionID> OrderCancelRequests { get; } = new Dictionary<string, SessionID>();
        public Dictionary<string, SessionID> MarketDataRequests { get; } = new Dictionary<string, SessionID>();
        public Dictionary<string, SessionID> Orders { get; } = new Dictionary<string, SessionID>(); 
    }
}