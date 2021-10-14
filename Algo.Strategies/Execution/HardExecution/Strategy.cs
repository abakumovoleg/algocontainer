using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Algo.Abstracts.Interfaces;
using Algo.Abstracts.Models;
using Algo.Strategies.Execution.HardExecution.PriceCalc;
using Microsoft.Extensions.Logging;

namespace Algo.Strategies.Execution.HardExecution
{
    public class Strategy : IStrategy
    {
        private readonly IStrategyContext _context;
        private readonly StrategyParameters _parameters;
        private readonly StrategyLog _log; 
        private MarketDepth _marketDepth;
         
        private readonly Calc _priceCalc = new Calc();
        private Order? _firstOrder;  

        public Strategy(IStrategyContext context, StrategyParameters parameters)
        {
            _context = context;
            _parameters = parameters;
            _log = context.Log;
            _marketDepth = new MarketDepth(_parameters.Security, new Quote[0], new Quote[0]);
        }
             
        private void PlaceOrder()
        {
            var executed = _context.Orders.Executed();
            var restVolume = Parameters.Volume - executed;

            _log.LogInfo($"Executed = {executed}, RestVolume = {restVolume}");
            
            if (executed >= Parameters.Volume)
            {
                _log.LogInfo("Executed >= Volume");

                _context.SetState(StrategyState.Completed);
                return;
            }

            if (restVolume < Parameters.Security.LotSize)
            {
                _log.LogInfo($"RestVolume < LotSize, {restVolume} < {Parameters.Security.LotSize}");

                _context.SetState(StrategyState.Completed);

                return;
            }

            if (_context.Orders.Count >= _parameters.MaxOrderCount)
            {
                _log.LogInfo("Orders.Count >= MaxOrderCount");
                _context.SetState(StrategyState.Failed);
                return;
            }


            decimal price;
            decimal size;

            if (_firstOrder == null)
            {
                var priceAndSize = _priceCalc.FindPriceAndOrderSize(_marketDepth, _parameters,
                    restVolume);

                _log.LogInfo($"PriceAndSize={priceAndSize}");

                if (priceAndSize.ResultStatus != ResultStatus.Success)
                {
                    _log.LogInfo(priceAndSize.ResultStatus.ToString());

                    _context.SetState(StrategyState.Failed);

                    return;
                }

                price = priceAndSize.Price;
                size = priceAndSize.OrderSize;
            }
            else
            {
                size = restVolume - restVolume % _parameters.Security.LotSize;

                var step = Parameters.Direction == Direction.Buy
                    ?  _parameters.PriceStep
                    : -_parameters.PriceStep;

                price = Math.Round(_firstOrder.Price + step * _context.Orders.Count, 4,
                    MidpointRounding.AwayFromZero);
            }
            
            var order = new Order
            {
                Price = price,
                Direction = _parameters.Direction,
                ClientCode = _parameters.Portfolio.ClientCode,
                Portfolio = _parameters.Portfolio,
                Security = _parameters.Security,
                OrderType = OrderType.Limit,
                TimeInForce = TimeInForce.CancelBalance,
                Volume = size,
                Comment = _parameters.Comment
            }; 

            _firstOrder ??= order;

            order.WhenDone()
                .ThenOnce(x =>
                {
                    PlaceOrder();
                }).Subscribe(_context);
                
            _context.RegisterOrder(order);
        }

        public StrategyState State => _context.State;

        public async Task Start(CancellationToken ct)
        {
            _log.LogInfo($"Start {Parameters.Direction} {Parameters.Security} {Parameters.Volume} with Hard strategy");

            _context.FailStrategyWhenOrderFailed();

            _context.SubscribeMarketDepth(Parameters.Security, x => _marketDepth = x);

            PlaceOrder();

            await _context.WaitUntilDoneOrCancel(ct);
        }

        public IStrategyParameters Parameters => _parameters;
        public IEnumerable<Order> Orders => _context.Orders;
        public string Log => _log.Log;
    }
}
