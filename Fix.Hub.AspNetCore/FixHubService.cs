using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Fix.Hub.AspNetCore
{
    public class FixHubService : IHostedService
    {
        private readonly FixHubServer _server;

        public FixHubService(FixHubServer server)
        {
            _server = server;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _server.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _server.Stop();
            return Task.CompletedTask;
        }
    }
}
