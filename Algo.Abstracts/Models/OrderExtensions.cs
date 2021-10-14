using System.Collections.Generic;
using System.Linq;

namespace Algo.Abstracts.Models
{
    public static class OrderExtensions
    {
        public static decimal Executed(this IEnumerable<Order> orders)
        {
            return orders.Where(x => x.OrderState != OrderState.Failed && x.OrderState != OrderState.None)
                .Sum(x => x.Volume - x.Balance);
        }

        public static decimal Balance(this IEnumerable<Order> orders)
        {
            return orders.Where(x => x.OrderState != OrderState.Failed && x.OrderState != OrderState.None)
                .Sum(x => x.Balance);
        }
    }
}