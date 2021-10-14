using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Algo.Strategies.Execution.Api.Abstracts;
using Algo.Strategies.Execution.Api.Controllers;
using Micro.Abstracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuickFix.Fields;

namespace Algo.Strategies.Execution.Api.Services
{
    public class StrategyManager
    {
        private readonly IServiceProvider _serviceProvider; 
        private readonly ILogger<StrategyManager> _logger; 

        public List<(Abstracts.Execution Execution, StrategyController Controller, CancellationTokenSource Cts)> Controllers { get; private set; } =
            new List<(Abstracts.Execution, StrategyController, CancellationTokenSource)>();

        public StrategyManager(IServiceProvider serviceProvider, ILogger<StrategyManager> logger)
        {
            _serviceProvider = serviceProvider; 
            _logger = logger;
        } 

        public async Task StartExecution(Abstracts.Execution execution)
        {
            var strategy = _serviceProvider.GetRequiredService<StrategyController>();
            var strategies = new List<IStrategy>();

            foreach (var s in execution.Strategies)
            {
                switch (s)
                {
                    case SoftExecutionStrategy soft :
                        strategies.Add(new SoftExecution.Strategy(new StrategyContext(strategy.Connector, strategy.MarketDepthProvider, strategy.Scheduler, _logger), 
                            new SoftExecution.StrategyParameters(execution.Security, execution.Portfolio,
                                soft.TimeInForce,
                                execution.Direction, execution.Volume, execution.Comment)));
                        break;
                    case HardExecutionStrategy hard:
                        strategies.Add(new HardExecution.Strategy(new StrategyContext(strategy.Connector, strategy.MarketDepthProvider, strategy.Scheduler, _logger),
                            new HardExecution.StrategyParameters(execution.Security, execution.Portfolio,
                                execution.Direction, execution.Volume, hard.PriceStep, hard.MaxOrderCount,
                                execution.Comment)));
                        break;
                    case GentleExecutionStrategy gentle:
                        var gs = new GentleExecution.Strategy(new StrategyContext(strategy.Connector, strategy.MarketDepthProvider, strategy.Scheduler, _logger), new GentleExecution.StrategyParameters(
                            execution.Security, execution.Portfolio,
                            gentle.TimeInForce, gentle.WaitTime, execution.Direction, execution.Volume,
                            execution.Comment));

                        strategies.Add(gs);
                        break;
                    default:
                        throw new Exception($"Invalid strategy type {s.Type}");
                }
            }

            var cts = new CancellationTokenSource();

            Controllers.Add((execution, strategy, cts));
            
            await strategy.Run(strategies.ToArray(), cts.Token);
        }

        public void AbortExecution(int id)
        {
            var cts = Controllers.First(x => x.Execution.ExecutionId == id).Cts;
            cts.Cancel();
        }

        ExecutionStrategyType GetType(IStrategy strategy)
        {
            return strategy switch
            {
                SoftExecution.Strategy _ => ExecutionStrategyType.Soft,
                HardExecution.Strategy _ => ExecutionStrategyType.Hard,
                GentleExecution.Strategy _ => ExecutionStrategyType.Gentle,
                _ => throw new Exception()
            };
        }

        public Abstracts.ExecutionDetails[] GetExecutionDetails()
        {
            var result = Controllers.Select(x =>
                {
                    var orderDetails = x.Controller.Strategies
                        .SelectMany(s => s.Orders.Select(o => new {Order = o, Strategy = s}))
                        .Select(z => new OrderDetails(GetType(z.Strategy), z.Order)).ToList();

                    var state = x.Controller.State == StrategyControllerState.Completed
                        ? ExecutionState.Completed
                        : x.Controller.State == StrategyControllerState.Failed 
                            ? ExecutionState.Failed
                            : ExecutionState.Active;

                    var executionLog =
                        string.Join($"{Environment.NewLine}", x.Controller.Strategies.Select(s => s.Log));

                    return new ExecutionDetails(x.Execution, orderDetails, state, executionLog);
                })
                .ToArray();

            return result;
        }
    }
}