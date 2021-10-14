using Algo.Abstracts.Models;

namespace Algo.Strategies.Execution.Api.Abstracts
{
    public class OrderDetails
    {
        public OrderDetails(ExecutionStrategyType strategyType, Order order)
        {
            StrategyType = strategyType;
            Order = order;
        }
        public ExecutionStrategyType StrategyType { get; set; }
        public Order Order { get; set; }
    }
}