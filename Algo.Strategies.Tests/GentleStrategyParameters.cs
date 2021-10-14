using Algo.Abstracts.Models;

namespace Algo.Strategies.Tests
{
    class GentleStrategyParameters
    {
        public decimal Volume { get; set; }
        public Direction Direction { get; set; }
        public int TimeInForce { get; set; }
        public int WaitTime { get; set; }
    }
}