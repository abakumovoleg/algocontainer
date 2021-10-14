using System.Collections.Generic;
using System.Linq;
using Algo.Abstracts.Models;

namespace Algo.Strategies.Execution.Api.Abstracts
{
    public class ExecutionDetails
    {
        public ExecutionDetails(Execution execution, List<OrderDetails> ordersDetails, ExecutionState state, string executionLog)
        {
            Execution = execution;
            OrdersDetails = ordersDetails;
            State = state;
            ExecutionLog = executionLog;
        }
        
        public ExecutionState State { get; }
        public string ExecutionLog { get; }
        public Execution Execution { get; }
        public List<OrderDetails> OrdersDetails { get;  }
        public decimal Executed => OrdersDetails.Select(x => x.Order).Executed();
        public decimal Balance => OrdersDetails.Select(x => x.Order).Balance();
    }
}