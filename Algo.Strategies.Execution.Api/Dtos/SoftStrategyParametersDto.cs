using System;
using Algo.Abstracts.Models;
using Algo.Strategies.Execution.Api.Abstracts;

namespace Algo.Strategies.Execution.Api.Dtos
{
    public class SoftStrategyParametersDto : IStrategyParametersBaseDto
    {    
        public int TimeInForce { get; set; }
        public ExecutionStrategyType Type { get; set; }
    }
}