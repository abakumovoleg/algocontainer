using Algo.Abstracts.Models;

namespace Algo.BackTesting
{
    public class MarketAction
    {
        public ActionType Type { get; set; }
        public MarketDepth MarketDepth { get; set; } 
        public int? Delay { get; set; }
    }
}