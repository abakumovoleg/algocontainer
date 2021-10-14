using System;

namespace Algo.Strategies.Execution.Api.Abstracts
{
    public class HardExecutionStrategy : ExecutionStrategy
    {
        public HardExecutionStrategy(int maxOrderCount, decimal priceStep)
            :base(ExecutionStrategyType.Hard)
        {
            if (maxOrderCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxOrderCount), "Should be more than 0");

            if (priceStep <= 0)
                throw new ArgumentOutOfRangeException(nameof(priceStep), "Should be more than 0");

            MaxOrderCount = maxOrderCount;
            PriceStep = priceStep;
        }
        public int MaxOrderCount { get; }
        public decimal PriceStep { get; }

        public override string ToString()
        {
            return $"Type = {Type}; MaxOrderCount = {MaxOrderCount}; PriceStep = {PriceStep}";
        }
    }
}