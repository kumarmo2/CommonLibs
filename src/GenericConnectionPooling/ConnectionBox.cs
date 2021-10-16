using System.Threading.Tasks;

namespace Core.CommonLibs.GenericConnectionPooling
{
    public class ConnectionBox<T> : IConnectionBox<T>
    where T : class
    {
        private AsyncConnectionPool<T> _pool;
        private readonly T _connection;

        internal ConnectionBox(AsyncConnectionPool<T> pool, T connection)
        {
            _connection = connection;
            _pool = pool;
        }
        public T Connection => _connection;

        public ValueTask DisposeAsync() => _pool.ReclaimConnection(_connection);
    }
}