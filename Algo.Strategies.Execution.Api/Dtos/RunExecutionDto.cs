using Algo.Abstracts.Models;

namespace Algo.Strategies.Execution.Api.Dtos
{
    public class RunExecutionDto
    {
        public PortfolioDto Portfolio { get; set; }
        public SecurityDto Security { get; set; }
        public Direction Direction { get; set; }
        public decimal Volume { get; set; }
        public string Comment { get; set; }
        public IStrategyParametersBaseDto[] Strategies { get; set; }
    }
}