using Microsoft.Extensions.DependencyInjection;

namespace CommonLibs.GrpcUtils
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGrpcChannel(this IServiceCollection services)
        {
            services.AddSingleton<IGrpcChannelFactory, GrpcChannelFactory>();
        }
    }
}