using System;
using Algo.Abstracts.Models;

namespace Algo.Strategies.Execution.Api.Dtos
{
    public class TradeDto
    {
        public string Id { get; set; }
        public string SecCode { get; set; }
        public DateTime TransactTime { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public Direction Direction { get; set; }
    }
}

