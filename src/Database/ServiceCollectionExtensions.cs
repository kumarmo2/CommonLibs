using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Options;

namespace CommonLibs.Database
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            // This overloaded method comes from Microsoft.Extensions.Options.ConfigurationExtensions.
            services.Configure<DbOptions>(configuration.GetSection(DbOptions.Key));
            services.AddSingleton<IDbConnectionFactory, PostgresDbConnectionFactory>();
        }
    }
}