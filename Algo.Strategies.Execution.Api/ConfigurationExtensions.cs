using System;
using Microsoft.Extensions.Configuration;

namespace Algo.Strategies.Execution.Api
{
    public static class ConfigurationExtensions
    {
        public static IConfigurationRoot BuildConfigurationRoot()
        {
            var aspnetcore = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var dotnetcore = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

            var environment = string.IsNullOrWhiteSpace(aspnetcore)
                ? dotnetcore
                : aspnetcore;

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile(
                    $"appsettings.{environment}.json",
                    optional: true)
                .Build();

            return configuration;
        }
    }
}