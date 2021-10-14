namespace Algo.Strategies.Execution.HardExecution.PriceCalc
{
    public class Result
    {
        public Result(decimal price, decimal orderSize)
        {
            Price = price;
            OrderSize = orderSize;
            ResultStatus = ResultStatus.Success;
        }

        public Result(ResultStatus resultStatus)
        { 
            ResultStatus = resultStatus;
        }

        public decimal Price { get; set; }
        public decimal OrderSize { get; set; }

        public ResultStatus ResultStatus { get; set; }

        public override string ToString()
        {
            return $"{ResultStatus}|Price={Price}|OrderSize={OrderSize}";
        }
    }
}