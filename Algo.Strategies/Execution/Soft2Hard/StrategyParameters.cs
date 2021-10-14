using System;
using Algo.Abstracts.Models;

namespace Algo.Strategies.Execution.Soft2Hard
{
    public class StrategyParameters : IStrategyParameters
    {
        public StrategyParameters(Security security, Portfolio portfolio, TimeSpan timeInForce, 
            Direction direction, decimal volume, string comment, int maxOrderCount, decimal priceStep)
        {
            Security = security ?? throw new ArgumentNullException(nameof(security));
            Portfolio = portfolio ?? throw new ArgumentNullException(nameof(portfolio));
            
            if (volume <= security.LotSize) throw new ArgumentOutOfRangeException(nameof(volume));

            TimeInForce = timeInForce;
            Direction = direction;
            Volume = volume;
            Comment = comment;
            MaxOrderCount = maxOrderCount;
            PriceStep = priceStep;
        }
         
        public Security Security { get;  }
        public Portfolio Portfolio { get;  }
        public TimeSpan TimeInForce { get; } 
        public Direction Direction { get;  }
        public decimal Volume { get; set; }
        public string Comment { get; }

        public decimal PriceStep { get;  }
        public int MaxOrderCount { get; }
    }
}