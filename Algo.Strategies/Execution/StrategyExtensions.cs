using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Algo.Abstracts.Interfaces;
using Algo.Abstracts.Models;
using Algo.Abstracts.Models.Messages;

namespace Algo.Strategies.Execution
{
    public static class StrategyExtensions
    {
        public static void CancelAllActiveOrders(this IConnector connector, OrderCollection orders)
        {
            foreach (var order in orders.Where(x => x.OrderState == OrderState.Active))
            {
                connector.CancelOrder(new CancelOrderMessage(order));
            }
        } 
    }
}