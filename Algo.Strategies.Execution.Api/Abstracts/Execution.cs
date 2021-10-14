using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Algo.Abstracts.Models;

namespace Algo.Strategies.Execution.Api.Abstracts
{
    public class Execution
    {
        private static volatile int _lastId;

        public Execution(Security security, Portfolio portfolio, Direction direction, decimal volume, string user, DateTime dateTime, List<ExecutionStrategy> strategies, string comment)
        {
            Security = security;
            Portfolio = portfolio;
            Direction = direction;
            Volume = volume;
            User = user;
            CreateDateTime = dateTime;
            Strategies = strategies;
            Comment = comment;
        }

        public int ExecutionId { get; } = Interlocked.Increment(ref _lastId);

        public Security Security { get; set; }
        public Portfolio Portfolio { get; set; }
        public Direction Direction { get; set; }
        public decimal Volume { get; }
        public string User { get; set; }
        public DateTime CreateDateTime { get; set; }
        public List<ExecutionStrategy> Strategies { get; set; }
        public string Comment { get; set; }
    }
}
