using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Algo.Strategies.Execution
{
    public class StrategyLog
    {
        private readonly ILogger _logger;
        private readonly StringBuilder _executionLog = new StringBuilder();

        public StrategyLog(ILogger logger)
        {
            _logger = logger;
        }

        public string Log => _executionLog.ToString();

        public void LogInfo(string text)
        {
            _executionLog.AppendLine($"{DateTime.Now:HH:mm:ss} {text}");
            _logger.LogInformation(text);
        }
    }
}