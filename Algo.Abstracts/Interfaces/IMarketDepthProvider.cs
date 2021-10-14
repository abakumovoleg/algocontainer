using Algo.Abstracts.Models;

namespace Algo.Abstracts.Interfaces
{
    public interface IMarketDepthProvider
    {
        void AddSubscription(Security security);
        MarketDepth Get(Security security);
    }
}