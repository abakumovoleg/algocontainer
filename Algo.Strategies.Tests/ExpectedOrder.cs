using Algo.Abstracts.Models;

namespace Algo.Strategies.Tests
{
    class ExpectedOrder
    {
        public decimal Volume { get; set; }
        public Direction Direction { get; set; }
        public decimal Price { get; set; }
    }
}