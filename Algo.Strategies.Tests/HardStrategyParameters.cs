using Algo.Abstracts.Models;

namespace Algo.Strategies.Tests
{
    class HardStrategyParameters
    {
        public decimal Volume { get; set; }
        public Direction Direction { get; set; } 
        public decimal PriceStep { get; set; }
        public int MaxOrderCount { get; set; }
    }
}