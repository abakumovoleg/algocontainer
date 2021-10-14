using System;
using Algo.Container.Serilog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using QuickFix;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace Algo.Strategies.Execution.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); }).UseSerilog(
                    (context, configuration) =>
                    {
                        configuration.Enrich.FromLogContext();
                        var settings = new SessionSettings($"FixConfig.{context.HostingEnvironment.EnvironmentName}.txt");

                        foreach (var sessionId in settings.GetSessions())
                        {
                            configuration.AddFixLoggingToFiles(sessionId.SenderCompID, sessionId.TargetCompID, "storage//logs");
                        }

                        configuration.WriteTo.Logger(x => x.WriteTo.Console());
                    });
        }
    }
}
