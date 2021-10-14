using Algo.Abstracts.Models;
using Algo.Abstracts.Models.Messages;

namespace Algo.Container.Fix.NtPro
{
    class MarketDepthSubscription
    {
        public Security Security { get; }
        public bool Subscribed { get; set; }
        public MarketDataSubscriptionType SubscriptionType { get; set; }

        public MarketDepthSubscription(Security security)
        {
            Security = security;
        }
    }
}