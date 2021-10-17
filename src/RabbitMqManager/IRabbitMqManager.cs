using System.Threading.Tasks;
using Core.CommonLibs.GenericConnectionPooling;
using RabbitMQ.Client;

namespace Core.CommonLibs.RabbitMq
{
    public interface IRabbitMqManager
    {

        ValueTask<IConnectionBox<IModel>> GetChannel();
        Task DeclareQueue(string queue, bool durable = true, bool exclusive = false,
            bool autoDelete = false);
    }
}