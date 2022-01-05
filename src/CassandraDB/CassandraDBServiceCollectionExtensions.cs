using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace CommonLibs.CassandraDB
{

    public static class CassandraDBServiceCollectionExtensions
    {
        public static void AddCassandraDB(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CassandraDBOptions>(configuration.GetSection(CassandraDBOptions.Key));
            services.AddSingleton<ICassandraConnectionFactory, CassandraConnectionFactory>();
        }
    }
}


