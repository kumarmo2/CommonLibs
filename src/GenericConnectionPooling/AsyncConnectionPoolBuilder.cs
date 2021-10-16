using System;
using System.Threading.Tasks;

namespace CommonLibs.GenericConnectionPooling
{
    public class AsyncConnectionPoolBuilder<T>
        where T : class
    {
        // TODO: create better builder.
        public IAsyncConnectionPool<T> Build(Func<Task<T>> connectionFactory, int poolSize)
        {
            return new AsyncConnectionPool<T>(connectionFactory, poolSize);
        }
    }
}