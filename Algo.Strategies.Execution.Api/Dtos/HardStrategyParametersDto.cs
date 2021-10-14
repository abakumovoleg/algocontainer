using System;
using Algo.Abstracts.Models;
using Algo.Strategies.Execution.Api.Abstracts;

namespace Algo.Strategies.Execution.Api.Dtos
{
    public class HardStrategyParametersDto : IStrategyParametersBaseDto
    {  
        public decimal PriceStep { get; set; } 
        public int MaxOrderCount { get; set; }
        public ExecutionStrategyType Type { get; set; }
    }
}