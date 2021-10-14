namespace Algo.Strategies.Execution
{
    public static class StrategyStateExstensions
    {
        public static bool IsFinal(this StrategyState state)
        {
            return state == StrategyState.Failed || state == StrategyState.Completed;
        }
    }
}