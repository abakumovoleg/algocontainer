using Serilog;
using System; 

namespace Algo.Container.Serilog
{
    public static class Extensions
    {
        public static LoggerConfiguration AddFixLoggingToFiles(this LoggerConfiguration configuration, string senderCompId, string targetCompId, string directory = "storage\\logs", int retainedFileCountLimit = 31)
        {
            var events = $@"events-{senderCompId}-{targetCompId}";
            var messages = $@"messages-{senderCompId}-{targetCompId}"; 

            configuration.WriteTo.Map("SourceContext", null, (s, sinkConfiguration) =>
            {
                if (s == events)
                    sinkConfiguration.Async(a => a.File($"{directory}//events-{senderCompId}-{targetCompId}.log",
                        rollingInterval: RollingInterval.Day, retainedFileCountLimit: retainedFileCountLimit));
                else if (s == messages)
                    sinkConfiguration.Async(a => a.File($"{directory}//messages-{senderCompId}-{targetCompId}.log",
                        rollingInterval: RollingInterval.Day, retainedFileCountLimit: retainedFileCountLimit));
            });

            return configuration;
        }

        public static LoggerConfiguration AddWebsocketLoggingToFiles(this LoggerConfiguration configuration, string directory = "storage\\logs", int retainedFileCountLimit = 31)
        {
            configuration.WriteTo.Map("SourceContext", null, (s, sinkConfiguration) =>
            {
                if (s == "Bcs.Connectors.NtPro.WebSocket.Client")
                    sinkConfiguration.Async(a => a.File($"{directory}//websocket.log",
                        rollingInterval: RollingInterval.Day, retainedFileCountLimit: retainedFileCountLimit));
            });

            return configuration;
        }
    }
}
