using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Algo.Abstracts.Interfaces;
using Algo.Abstracts.Models;
using Microsoft.Extensions.Logging;

namespace Algo.Strategies.Execution
{
    
    public class StrategyController : IDisposable
    { 
        private readonly ILogger _logger;

        private IDisposable? _mdSubscription;

        private readonly SemaphoreSlim _disposeEvent = new SemaphoreSlim(0, 1);

        public StrategyControllerState State { get; private set; } 

        public List<string> Errors = new List<string>();


        public EventLoopScheduler Scheduler { get; } = new EventLoopScheduler();
        public IConnector Connector { get; }
        public IMarketDepthProvider MarketDepthProvider { get; }

        public StrategyController(IConnector connector, IMarketDepthProvider marketDepthProvider, ILogger logger)
        {
            Connector = connector;
            MarketDepthProvider = marketDepthProvider;
            _logger = logger;  
        }

        public IStrategy[]? Strategies { get; private set; } 
        
        public async Task Run(IStrategy[] strategies, CancellationToken ct)
        {
            State = StrategyControllerState.Active;

            Strategies = strategies;

            _logger.LogInformation($"{nameof(StrategyController)}.{nameof(Run)} starting");

            var security = strategies.First().Parameters.Security;

            MarketDepthProvider.AddSubscription(strategies.First().Parameters.Security); 

            var mre = new ManualResetEvent(false);

            _mdSubscription = Connector.MarketDepthChanged
                .Where(x => x.Security != null && x.Security.Equals(security))
                .Subscribe(x =>
                {
                    if (x.Security == null || !x.Security.Equals(security))
                        return;

                    mre.Set();

                    _mdSubscription?.Dispose();
                }); 

            var md = MarketDepthProvider.Get(security);
            
            if (md != null)
                mre.Set();

            // waiting for market depth
            if ( !mre.WaitOne(1000))
            {
                _logger.LogError("Market Depth timeout");

                Errors.Add("Market Depth timeout"); 

                Dispose();

                return;
            }

            foreach (var strategy in strategies)
            {
                var volume = strategies.First().Parameters.Volume -
                              GetOrders().Where(x => x.OrderState != OrderState.Failed)
                                 .Sum(x => x.Volume - x.Balance);
                
                strategy.Parameters.Volume = volume;

                await strategy.Start(ct);

                if (ct.IsCancellationRequested || 
                    strategy.State == StrategyState.Completed)
                {
                    Dispose();
                    return;
                }
            }

            Dispose();
        }
          
        public Order[] GetOrders()
        {
            return Strategies.SelectMany(x => x.Orders).ToArray();
        } 
        
        public void Dispose()
        {
            _mdSubscription?.Dispose();  

            Scheduler.Dispose();
             
            if(_disposeEvent.CurrentCount == 0)
                _disposeEvent.Release(1);

            State = Strategies.Any(x => x.State == StrategyState.Completed) 
                ? StrategyControllerState.Completed 
                : StrategyControllerState.Failed;
        }

       
    }
}
