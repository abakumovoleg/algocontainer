using Microsoft.Extensions.Logging;
using QuickFix;

namespace Algo.Container.Fix.NtPro
{
    public class DefaultLog : ILog
    {
        private readonly ILogger _messageLogger;
        private readonly ILogger _eventLogger;

        public DefaultLog(ILogger messageLogger, ILogger eventLogger)
        {
            _messageLogger = messageLogger;
            _eventLogger = eventLogger;
        }

        public void Dispose()
        {
        }

        public void Clear()
        {
        }

        public void OnIncoming(string msg)
        {
            _messageLogger.LogInformation(msg);
        }

        public void OnOutgoing(string msg)
        {
            _messageLogger.LogInformation(msg);
        }

        public void OnEvent(string s)
        {
            _eventLogger.LogInformation(s);
        }
    }
}