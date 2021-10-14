using Algo.Abstracts.Models;

namespace Algo.Container
{
    class MarketDepthSubscription
    {
        public Security Security { get; }
        public bool Subscribed { get; set; }

        public MarketDepthSubscription(Security security)
        {
            Security = security;
        }
    }
}