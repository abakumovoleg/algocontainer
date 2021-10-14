using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Algo.Abstracts.Models;

namespace Algo.Strategies.Execution.GentleExecution
{
    public class Strategy : IStrategy
    { 
        private readonly StrategyParameters _parameters;
        private readonly IStrategyContext _context;
        private readonly StrategyLog _log;
        private MarketDepth _marketDepth; 

        public Strategy(IStrategyContext context, StrategyParameters parameters)
        {
            _context = context; 
            Parameters = _parameters = parameters;
            _log = context.Log;
            _marketDepth = new MarketDepth(_parameters.Security, new Quote[0], new Quote[0]);
        } 
           
        private void PlaceOrder()
        {
            var quote = (_parameters.Direction == Direction.Buy ? _marketDepth.Bids : _marketDepth.Asks).FirstOrDefault();

            if (quote == null)
            {
                _log.LogInfo("MarketDepth is empty");
                _context.SetState(StrategyState.Failed);
                return;
            }

            _log.LogInfo($"Price={quote.Price}");

            var order = new Order
            {
                Price = quote.Price,
                Direction = _parameters.Direction,
                ClientCode = _parameters.Portfolio.ClientCode,
                Portfolio = _parameters.Portfolio,
                Security = _parameters.Security,
                OrderType = OrderType.Limit,
                TimeInForce = TimeInForce.PutInQueue,
                Volume = _parameters.Volume - _context.Orders.Executed(),
                Comment = _parameters.Comment
            }; 

            var waitTimeExpiredCts = new CancellationTokenSource();

            _context.Schedule(_parameters.WaitTime, () =>
            {
                _log.LogInfo($"WaitTime {_parameters.WaitTime} expired");
                  
                _context.CancelOrder(order);
            }, waitTimeExpiredCts.Token);

            order.WhenDone().ThenOnce(x =>
            {
                waitTimeExpiredCts.Cancel();

                if (_context.Orders.Executed() >= _parameters.Volume)
                    _context.SetState(StrategyState.Completed);
                else
                    PlaceOrder();
            }).Subscribe(_context);


            _context.RegisterOrder(order); 
        }


        public StrategyState State => _context.State;

        public async Task Start(CancellationToken ct)
        {
            _log.LogInfo(
                $"Start {Parameters.Direction} {Parameters.Security} {Parameters.Volume} with Gentle strategy");

            _context.FailStrategyWhenOrderFailed();

            _context.Schedule(_parameters.TimeInForce,
                () =>
                {
                    _log.LogInfo($"TimeInForce {_parameters.TimeInForce} expired");
                    _context.SetState(StrategyState.Failed);
                }, CancellationToken.None);

            _context.SubscribeMarketDepth(Parameters.Security, x => _marketDepth = x);

            PlaceOrder();

            await _context.WaitUntilDoneOrCancel(ct);
        }

        public IStrategyParameters Parameters { get; }
        public IEnumerable<Order> Orders => _context.Orders;
        public string Log => _log.Log;
    }
}
