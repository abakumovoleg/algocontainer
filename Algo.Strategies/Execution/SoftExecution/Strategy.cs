using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Algo.Abstracts.Models;

namespace Algo.Strategies.Execution.SoftExecution
{
    public class Strategy : IStrategy
    {
        private readonly IStrategyContext _context;
        private readonly StrategyParameters _parameters;
        
        private MarketDepth _marketDepth;
        private readonly StrategyLog _log;
        

        public Strategy(IStrategyContext context, StrategyParameters parameters)
        {
            _context = context;
            _parameters = parameters;
            _log = context.Log;
            _marketDepth = new MarketDepth(_parameters.Security, new Quote[0], new Quote[0]);
        }
          
        private void PlaceOrder()
        { 
            var quote = (_parameters.Direction == Direction.Buy ? _marketDepth.Asks : _marketDepth.Bids).FirstOrDefault();

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
                Volume = _parameters.Volume,
                Comment = _parameters.Comment
            }; 

            order.WhenDone()
                .ThenOnce(x => _context.Finish(Parameters.Volume))
                .Subscribe(_context);

            _context.RegisterOrder(order); 
        }

        public StrategyState State => _context.State;

        public async Task Start(CancellationToken ct)
        {
            _log.LogInfo($"Start {Parameters.Direction} {Parameters.Security} {Parameters.Volume} with Soft strategy");

            _context.FailStrategyWhenOrderFailed();

            _context.SubscribeMarketDepth(Parameters.Security, x => _marketDepth = x);

            _context.Schedule(_parameters.TimeInForce, () =>
            {
                _log.LogInfo($"TimeInForce {_parameters.TimeInForce} expired");

                _context.SetState(StrategyState.Failed);
            }, CancellationToken.None);

            PlaceOrder();

            await _context.WaitUntilDoneOrCancel(ct);
        }

        public IStrategyParameters Parameters => _parameters;
        public IEnumerable<Order> Orders => _context.Orders;
        public string Log => _log.Log;
    }
}
