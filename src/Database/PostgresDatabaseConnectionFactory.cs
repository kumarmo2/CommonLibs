using System.Data;
using Microsoft.Extensions.Options;
using Npgsql;

namespace CommonLibs.Database
{
    public class PostgresDbConnectionFactory : IDbConnectionFactory
    {
        private readonly DbOptions _dbOptions;
        public PostgresDbConnectionFactory(IOptions<DbOptions> dbOptions)
        {
            _dbOptions = dbOptions.Value;
        }
        public IDbConnection GetDbConnection()
        {
            // TODO: added connection pooling.
            return new NpgsqlConnection(_dbOptions.ConnectionString);
        }
    }
}
