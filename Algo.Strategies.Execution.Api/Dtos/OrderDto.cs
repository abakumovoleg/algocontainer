using System;
using Algo.Abstracts.Models;
using Algo.Strategies.Execution.Api.Abstracts;

namespace Algo.Strategies.Execution.Api.Dtos
{
    public class OrderDto
    {
        public decimal Price { get; set; }
        public string UserOrderId { get; set; }
        public string SecCode { get; set; }
        public decimal Volume { get; set; }
        public decimal Executed { get; set; }
        public decimal Balance { get; set; }
        public OrderState OrderState { get; set; }
        public DateTime Time { get; set; }
        public Direction Direction { get; set; }
        public string[] Messages { get; set; }
        public ExecutionStrategyType Strategy { get; set; }
    }
}