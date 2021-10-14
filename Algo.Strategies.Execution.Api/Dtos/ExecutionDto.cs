using System;
using System.Collections.Generic;
using Algo.Abstracts.Models;
using Algo.Strategies.Execution.Api.Abstracts;

namespace Algo.Strategies.Execution.Api.Dtos
{
    public class ExecutionDto
    {
        public string SecCode { get; set; }
        public string Account { get; set; }
        public string User { get; set; }
        public Direction Direction { get; set; }
        public decimal Volume { get; set; }
        public DateTime CreateDate { get; set; }

        public ExecutionState State { get; set; }
        public List<OrderDto> Orders { get; set; }

        public decimal Executed { get; set; }
        public decimal Balance { get; set; }
        public int Id { get; set; }
        public string Parameters { get; set; }
        public string ExecutionLog { get; set; }

        //public List<StrategyDto> Strategies { get; set; } = new List<StrategyDto>();
    }
}