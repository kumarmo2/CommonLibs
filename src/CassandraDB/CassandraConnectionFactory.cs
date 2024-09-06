using Microsoft.Extensions.Options;
using Cassandra;
using System;
using System.Threading.Tasks;

namespace CommonLibs.CassandraDB
{
    public class CassandraConnectionFactory : ICassandraConnectionFactory
    {
        private readonly CassandraDBOptions _dbOptions;
        private Lazy<Task<ISession>> _lazySession;

        public CassandraConnectionFactory(IOptions<CassandraDBOptions> options)
        {
            _dbOptions = options.Value;
            _lazySession = new Lazy<Task<ISession>>(async () =>
                    {
                        var cluster = Cluster.Builder()
                        .AddContactPoint(_dbOptions.ConnectionString)
                        .Build();
                        return await cluster.ConnectAsync();
                    });
        }

        public async Task<ISession> GetConnection()
        {
            return await _lazySession.Value;
        }
    }
}

