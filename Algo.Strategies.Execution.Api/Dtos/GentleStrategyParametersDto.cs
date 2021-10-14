using Algo.Abstracts.Models;
using Algo.Strategies.Execution.Api.Abstracts;

namespace Algo.Strategies.Execution.Api.Dtos
{
    public class GentleStrategyParametersDto : IStrategyParametersBaseDto
    {
       
        public ExecutionStrategyType Type { get; set; }
        public int TimeInForce { get; set; }
        public int WaitTime { get; set; }
    }
}