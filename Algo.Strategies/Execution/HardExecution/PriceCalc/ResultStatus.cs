namespace Algo.Strategies.Execution.HardExecution.PriceCalc
{
    public enum ResultStatus
    {
        Success,
        MarketDepthIsEmpty,
        PriceOutOfRange,
        LessThanLotSize
    }
}