using System;

namespace Algo.Strategies.Execution.Api.Abstracts
{
    public class SoftExecutionStrategy : ExecutionStrategy
    {
        public SoftExecutionStrategy(TimeSpan timeInForce)
            : base(ExecutionStrategyType.Soft)
        {
            if (timeInForce.TotalSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(timeInForce), "Should be more than 0");

            TimeInForce = timeInForce;
        }

        public TimeSpan TimeInForce { get; }

        public override string ToString()
        {
            return $"Type = {Type}; TimeInForce = {TimeInForce}";
        }
    }
}