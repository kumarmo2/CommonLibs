using System;
using RabbitMQ.Client;
using Core.CommonLibs.GenericConnectionPooling;
using System.Threading.Tasks;

namespace Core.CommonLibs.RabbitMq
{
    // Create a single manager for the application.
    public class RabbitMqManager : IRabbitMqManager
    {
        // For now, we will make a single connectionOnly.
        private readonly Lazy<IConnection> _connection;
        // This is the rabbitmq channel pool.
        private readonly IAsyncConnectionPool<IModel> _channelFactory;
        public RabbitMqManager()
        {
            _connection = new Lazy<IConnection>(() =>
            {
                // TODO: make address configurable.
                var factory = new ConnectionFactory() { HostName = "localhost", DispatchConsumersAsync = true };
                return factory.CreateConnection();
            });

            _channelFactory = new AsyncConnectionPoolBuilder<IModel>().Build(ChannelProducer, 2);
        }

        private Task<IModel> ChannelProducer()
        {
            return Task.FromResult(_connection.Value.CreateModel());
        }

        public ValueTask<IConnectionBox<IModel>> GetChannel() => _channelFactory.GetConnection();

        public async Task DeclareQueue(string queue, bool durable = true, bool exclusive = false,
            bool autoDelete = false)
        {
            await using (var channelBox = await _channelFactory.GetConnection())
            {
                var channel = channelBox.Connection;
                channel.QueueDeclare(queue, durable, exclusive, autoDelete);
            }
        }

    }
}