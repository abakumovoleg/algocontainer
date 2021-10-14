using Microsoft.Extensions.DependencyInjection;

namespace Fix.Hub.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        public static void UseFixHub(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<FixHubServer>();
            serviceCollection.AddHostedService<FixHubService>();
        }
    }
}