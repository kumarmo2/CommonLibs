
using System.Threading.Tasks;
using Cassandra;

namespace CommonLibs.CassandraDB
{
    public interface ICassandraConnectionFactory
    {

        Task<ISession> GetConnection();
    }

}

