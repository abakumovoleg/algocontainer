using System;

namespace Algo.Strategies.Execution.Api.Abstracts
{
    public class GentleExecutionStrategy : ExecutionStrategy
    {
        public TimeSpan TimeInForce { get; }
        public TimeSpan WaitTime { get; }

        public GentleExecutionStrategy(TimeSpan timeInForce, TimeSpan waitTime)
            : base(ExecutionStrategyType.Gentle)
        {
            if (timeInForce.TotalSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(timeInForce), "Should be more than 0");
            
            if (waitTime.TotalSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(timeInForce), "Should be more than 0");

            if(waitTime >= timeInForce)
                throw new ArgumentException($"WaitTime >= TimeInForce, {timeInForce} >= {waitTime}");

            TimeInForce = timeInForce;
            WaitTime = waitTime;
        } 

        public override string ToString()
        {
            return $"Type = {Type}; TimeInForce = {TimeInForce}; WaitTime = {WaitTime}";
        }
    }
}