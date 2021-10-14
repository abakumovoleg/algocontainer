using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Algo.Abstracts.Models; 

namespace Algo.Strategies.Execution.Soft2Hard
{
    public class Strategy : IStrategy
    {
        private readonly IStrategyContext _context;
        private readonly StrategyParameters _parameters;
        private SoftExecution.Strategy? _soft;
        private HardExecution.Strategy? _hard;

        public Strategy(IStrategyContext context, StrategyParameters parameters)
        {
            _context = context;
            _parameters = parameters;
        }

        public StrategyState State => _context.State;

        public async Task Start(CancellationToken ct)
        {
            _soft = new SoftExecution.Strategy(_context.CreateChildContext(),
                new SoftExecution.StrategyParameters(Parameters.Security, Parameters.Portfolio, _parameters.TimeInForce,
                    Parameters.Direction, Parameters.Volume, Parameters.Comment));

            await _soft.Start(ct);

            if (_soft.State == StrategyState.Failed)
            {
                _hard = new HardExecution.Strategy(_context.CreateChildContext(),
                    new HardExecution.StrategyParameters(Parameters.Security, Parameters.Portfolio,
                        Parameters.Direction, Parameters.Volume - _soft.Orders.Executed(),  _parameters.PriceStep, _parameters.MaxOrderCount,
                        Parameters.Comment));

                await _hard.Start(ct);

                _context.SetState(_hard.State);
            }
            else
                _context.SetState(_soft.State);
        }

        public IStrategyParameters Parameters => _parameters;
        public IEnumerable<Order> Orders => _context.Orders
            .Concat(_soft?.Orders ?? new Order[0])
            .Concat(_hard?.Orders ?? new Order[0]);
        public string Log => _context.Log.Log;
    }
}
