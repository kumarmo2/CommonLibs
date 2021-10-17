using Core.CommonLibs.RabbitMq;
using Microsoft.Extensions.DependencyInjection;

namespace Core.CommonLibs
{
    public static class IServiceCollectionExtensions
    {
        public static void AddRabbitMq(this IServiceCollection services)
        {
            // TODO: Add configuration.
            services.AddSingleton<IRabbitMqManager, RabbitMqManager>();
        }
    }
}