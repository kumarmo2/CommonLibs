using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Core.CommonLibs.GenericConnectionPooling
{
    internal class AsyncConnectionPool<T> : IAsyncConnectionPool<T>
    where T : class
    {
        private int _poolSize;
        // The total number of object instantiated.
        private int _currentTotalObjectCount;
        private readonly ChannelReader<T> _channelReader;
        private readonly ChannelWriter<T> _channelWriter;
        private readonly Func<Task<T>> _connectionFactory;

        internal AsyncConnectionPool(Func<Task<T>> connectionFactory, int poolSize)
        {
            if (connectionFactory is null)
            {
                throw new ArgumentNullException(nameof(connectionFactory));
            }
            if (poolSize < 1)
            {
                throw new ArgumentException($"Invalid {nameof(poolSize)}");
            }

            _connectionFactory = connectionFactory;
            _poolSize = poolSize;

            var channel = Channel.CreateBounded<T>(poolSize);
            _channelReader = channel.Reader;
            _channelWriter = channel.Writer;
            // TODO: in case a connection has been inactive for a while, we should be able to somehow close it.

        }

        internal ValueTask ReclaimConnection(T instance)
        {
            return _channelWriter.WriteAsync(instance);
        }
        public async ValueTask<IConnectionBox<T>> GetConnection()
        {
            if (_channelReader.TryRead(out var connection) && connection != null)
            {
                return new ConnectionBox<T>(this, connection);
            }
            var _currentTotalConnectionInitiated = Interlocked.CompareExchange(ref _currentTotalObjectCount, _poolSize, _poolSize);
            if (_currentTotalConnectionInitiated >= _poolSize)
            {
                while (await _channelReader.WaitToReadAsync())
                {
                    if (_channelReader.TryRead(out connection))
                    {
                        return new ConnectionBox<T>(this, connection);
                    }
                }
            }
            Interlocked.Increment(ref _currentTotalObjectCount);
            connection = await _connectionFactory();
            return new ConnectionBox<T>(this, connection);
        }
    }
}