using System;
using Algo.Abstracts.Models;

namespace Algo.Strategies.Execution.HardExecution
{
    public class StrategyParameters : IStrategyParameters
    {
        public StrategyParameters(Security security, Portfolio portfolio, Direction direction, decimal volume, decimal priceStep, int maxOrderCount, string comment)
        {
            Security = security;
            Portfolio = portfolio;
            Direction = direction;
            Volume = volume;
            PriceStep = priceStep;
            MaxOrderCount = maxOrderCount;
            Comment = comment;
        }

        public Security Security { get;   }
        public Portfolio Portfolio { get;   }
        public Direction Direction { get;   }
        public decimal Volume { get; set; }
        public decimal PriceStep { get; }
        public string Comment { get;  }
        public int MaxOrderCount { get; }
         
        public override string ToString()
        {
            return
                $"{Security}|{Portfolio}|{Direction}|Volume={Volume}|Comment={Comment}";
        }
    }
}