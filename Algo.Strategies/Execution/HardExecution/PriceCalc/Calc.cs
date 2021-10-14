using System;
using Algo.Abstracts.Models;

namespace Algo.Strategies.Execution.HardExecution.PriceCalc
{
    public class Calc
    {
        public Result FindPriceAndOrderSize(MarketDepth marketDepth, StrategyParameters parameters, decimal restVolume)
        {
            var prices = parameters.Direction == Direction.Buy ? marketDepth.Asks : marketDepth.Bids;
            var desiredSize = restVolume; 

            var volume = 0M;
            var findPrice = 0M;

            if (desiredSize < parameters.Security.LotSize)
                return new Result(ResultStatus.LessThanLotSize);

            var realSize = desiredSize - desiredSize % parameters.Security.LotSize;

            foreach (var price in prices)
            {
                volume += price.Volume;
                findPrice = price.Price;

                if (realSize <= volume)
                    break;
            }

            if (volume >= realSize)
                return new Result(findPrice, realSize);

            if (volume >= parameters.Security.LotSize)
                return new Result(findPrice, volume - volume % parameters.Security.LotSize);

            return new Result(ResultStatus.MarketDepthIsEmpty);
        }
    }
}