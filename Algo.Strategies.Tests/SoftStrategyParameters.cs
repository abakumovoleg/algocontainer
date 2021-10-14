using Algo.Abstracts.Models;

namespace Algo.Strategies.Tests
{
    class SoftStrategyParameters
    {
        public decimal Volume { get; set; }
        public Direction Direction { get; set; } 
        public int TimeInForce { get; set; }
    }
}