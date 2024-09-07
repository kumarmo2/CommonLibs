using Microsoft.Extensions.DependencyInjection;

namespace CommonLibs.Utils.Id
{
    public static class IdServiceClassExtensions
    {
        public static void AddIdFactory(this IServiceCollection services)
        {
            services.AddSingleton<IIdFactory, IdFactory>();
        }
    }
}
