using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Algo.Abstracts.Models;

namespace Algo.Strategies.Execution
{
    public interface IStrategy
    {
        StrategyState State { get; } 
        Task Start(CancellationToken ct);
        IStrategyParameters Parameters { get; } 
        IEnumerable<Order> Orders { get; }
        string Log { get; }
    }
}