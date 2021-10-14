namespace Algo.Strategies.Execution.Api.Abstracts
{
    public class ExecutionStrategy
    {
        public ExecutionStrategy(ExecutionStrategyType type)
        {
            Type = type;
        }
        public ExecutionStrategyType Type { get;  }
    }
}