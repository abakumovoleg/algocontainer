using System;

namespace Algo.Abstracts.Models
{
    public class Trade
    {
        public string Id { get; set; }
        public Security Security { get; set; }
        public DateTime TransactTime { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }

        public Direction Direction { get; set; }

        public override string ToString()
        {
            return $"{Id} {Security} {TransactTime} {Quantity} {Price}";
        }
    }
}