using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Algo.Abstracts.Models
{
    public class Order 
    { 
        public long? Id { get; set; }
        public string UserOrderId { get; set; } = Guid.NewGuid().ToString();
        public string OrderId { get; set; }
        public string ClientCode { get; set; }
        public decimal Volume { get; set; }
        public decimal Balance { get; set; }
        public decimal Price { get; set; }
        public Direction Direction { get; set; }
        public OrderState OrderState { get; set; }
        public OrderType OrderType { get; set; }

        public Security Security { get; set; }

        public Portfolio Portfolio { get; set; }

        public TimeInForce TimeInForce { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;

        public HashSet<string> Messages { get; }= new HashSet<string>();

        public string Comment { get; set; }

        public override string ToString()
        {
            return $"{Security} {Volume} {Price} {Direction} {OrderType} {UserOrderId} {OrderState} {Balance} {string.Join(",", Messages)}";
        }
    }
}
