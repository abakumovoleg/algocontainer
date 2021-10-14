using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Algo.Abstracts.Models;

namespace Algo.Strategies.Execution
{
    /// <summary>
    /// Thread safe collection of Orders
    /// </summary>
    public class OrderCollection : IEnumerable<Order>
    {
        private readonly ConcurrentDictionary<Order, Order> _orders = new ConcurrentDictionary<Order, Order>();

        public int Count => _orders.Count;
        public void Add(Order order)
        {
            _orders[order] = order;
        }

        public bool Contains(Order order)
        {
            return _orders.ContainsKey(order);
        }

        public IEnumerator<Order> GetEnumerator()
        {
            return _orders.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}