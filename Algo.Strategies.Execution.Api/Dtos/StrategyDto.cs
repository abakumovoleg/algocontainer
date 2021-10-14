using System;
using Algo.Abstracts.Models;

namespace Algo.Strategies.Execution.Api.Dtos
{
    public class StrategyDto
    {
        public string State { get; set; } 
        public Direction Direction { get; set; }
        public decimal Volume { get; set; }

        public OrderDto[] Orders { get; set; } = new OrderDto[0];
        public TradeDto[] Trades { get; set; } = new TradeDto[0];
        public int? MaxOrderCount { get; set; }
        public TimeSpan? TimeInForce { get; set; }
    }
}