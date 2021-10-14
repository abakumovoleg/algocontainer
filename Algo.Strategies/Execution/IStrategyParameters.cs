using System;
using Algo.Abstracts.Models;

namespace Algo.Strategies.Execution
{
    public interface IStrategyParameters
    { 
        Direction Direction { get; }
        Security Security { get; }
        Portfolio Portfolio { get; }

        decimal Volume { get; set; } 
        string Comment { get; }
    }
}