using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Algo.Abstracts.Interfaces;
using Algo.Abstracts.Models;
using Algo.Container;
using Algo.Strategies.Execution.Api.Abstracts;
using Algo.Strategies.Execution.Api.Dtos;
using Algo.Strategies.Execution.Api.Services;
using Micro.Abstracts;
using Microsoft.AspNetCore.Mvc;

namespace Algo.Strategies.Execution.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExecutionController : ControllerBase
    {
        private readonly StrategyManager _strategyManager;
        private readonly ISecurityProvider _securityProvider;
        private readonly ICurrentUserAccessor _currentUserAccessor;

        public ExecutionController(StrategyManager strategyManager, ISecurityProvider securityProvider, ICurrentUserAccessor currentUserAccessor)
        {
            _strategyManager = strategyManager;
            _securityProvider = securityProvider;
            _currentUserAccessor = currentUserAccessor;
        }

        [HttpPost]
        public Task<ResponseDto<RunExecutionResponseDto>> Run([FromBody]RunExecutionDto runExecution)
        {
            var user = _currentUserAccessor.Get();
            var security = _securityProvider.Get(runExecution.Security.SecCode, runExecution.Security.ClassCode);

            if(security == null)
                throw new Exception($"Security '{runExecution.Security.ClassCode}.{runExecution.Security.SecCode}' not found");


            var portfolio = new Portfolio
                { ClientCode = runExecution.Portfolio.ClientCode, Name = runExecution.Portfolio.Name };

            var strategies = new List<ExecutionStrategy>();
            
            foreach (var parameters in runExecution.Strategies)
            { 
                switch (parameters)
                {
                    case SoftStrategyParametersDto soft:
                        strategies.Add(new SoftExecutionStrategy(TimeSpan.FromSeconds(soft.TimeInForce)));
                        break;

                    case HardStrategyParametersDto hard:
                        strategies.Add(new HardExecutionStrategy(hard.MaxOrderCount, hard.PriceStep));
                        break;
                    case GentleStrategyParametersDto gentle:
                        strategies.Add(new GentleExecutionStrategy(TimeSpan.FromSeconds(gentle.TimeInForce),
                            TimeSpan.FromSeconds(gentle.WaitTime)));
                        break;
                    default:
                        throw new Exception($"Invalid type {parameters.GetType()}");
                }
            }

            var execution = new Abstracts.Execution(security, portfolio, runExecution.Direction,
                runExecution.Volume, user.Name, DateTime.Now, strategies, runExecution.Comment);

            _ = _strategyManager.StartExecution(execution);

            return Task.FromResult(new ResponseDto<RunExecutionResponseDto>(new RunExecutionResponseDto
            {
                ExecutionId = execution.ExecutionId
            }));
        }

        [HttpPost("abort")]
        public Task Abort([FromBody]AbortExecutionDto dto)
        {
            _strategyManager.AbortExecution(dto.ExecutionId);
            return Task.CompletedTask;
        }
    
        [HttpGet]
        public ResponseDto<ExecutionDto[]> Get()
        {
            var details = _strategyManager.GetExecutionDetails();
            var result = details.Select(x => new ExecutionDto
            {
                Id = x.Execution.ExecutionId,
                Parameters = string.Join(",", x.Execution.Strategies),
                Account = x.Execution.Portfolio.ClientCode,
                Direction = x.Execution.Direction,
                User = x.Execution.User,
                CreateDate = x.Execution.CreateDateTime,
                State = x.State,
                Executed = x.Executed,
                Balance = x.Balance,
                SecCode = x.Execution.Security.Code,
                Volume = x.Execution.Volume,
                ExecutionLog = x.ExecutionLog,
                Orders = x.OrdersDetails
                    .OrderBy(z => z.Order.Time)
                    .Select(od =>
                    {
                        var o = od.Order;

                        return new OrderDto
                        {
                            Direction = o.Direction,
                            Price = o.Price,
                            UserOrderId = o.UserOrderId,
                            SecCode = o.Security.Code,
                            Volume = o.Volume,
                            Executed = o.Volume - o.Balance,
                            Balance = o.Balance,
                            OrderState = o.OrderState,
                            Time = o.Time,
                            Messages = o.Messages.ToArray(),
                            Strategy = od.StrategyType
                        };
                    }).ToList()
            }).ToArray();
            
            return new ResponseDto<ExecutionDto[]>(result); 
        }
    }
}
