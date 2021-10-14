using System;
using Algo.Abstracts.Models;

namespace Algo.Strategies.Execution.GentleExecution
{
    public class StrategyParameters : IStrategyParameters
    {
        public StrategyParameters(Security security, Portfolio portfolio, TimeSpan timeInForce, TimeSpan waitTime,
            Direction direction, decimal volume, string comment)
        {
            Security = security ?? throw new ArgumentNullException(nameof(security));
            Portfolio = portfolio ?? throw new ArgumentNullException(nameof(portfolio));

            if (volume <= security.LotSize) throw new ArgumentOutOfRangeException(nameof(volume));

            if (timeInForce <= waitTime)
                throw new ArgumentOutOfRangeException(nameof(timeInForce),
                    $"TimeInForce <= WaitTime, {timeInForce} <= {waitTime}");

            TimeInForce = timeInForce;
            WaitTime = waitTime;
            Direction = direction;
            Volume = volume;
            Comment = comment; 
        }
         
        public Security Security { get;  }
        public Portfolio Portfolio { get;  }
        public TimeSpan TimeInForce { get; }
        public TimeSpan WaitTime { get; }
        public Direction Direction { get;  }
        public decimal Volume { get; set; }
        public string Comment { get; }  
    }
}