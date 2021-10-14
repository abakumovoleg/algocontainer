using Microsoft.Extensions.Logging;
using QuickFix;

namespace Algo.Container.Fix.NtPro
{
    public class DefaultLogFactory : ILogFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public DefaultLogFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public ILog Create(SessionID sessionID)
        {
            return new DefaultLog(_loggerFactory.CreateLogger($"messages-{sessionID.SenderCompID}-{sessionID.TargetCompID}"),
                _loggerFactory.CreateLogger($"events-{sessionID.SenderCompID}-{sessionID.TargetCompID}"));
        }
    }
}