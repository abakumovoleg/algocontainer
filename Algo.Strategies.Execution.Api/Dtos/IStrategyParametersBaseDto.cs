using Algo.Abstracts.Models;

namespace Algo.Strategies.Execution.Api.Dtos
{
    public interface IStrategyParametersBaseDto
    { 
        Abstracts.ExecutionStrategyType Type { get; set; }
    }
}