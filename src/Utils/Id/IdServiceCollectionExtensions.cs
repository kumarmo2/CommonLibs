namespace CommonLibs.Utils.Id;
using Microsoft.Extensions.DependencyInjection;


public static class IdServiceClassExtensions
{
    public static void AddIdFactory(this IServiceCollection services)
    {
        services.AddSingleton<IIdFactory, IdFactory>();
    }
}
