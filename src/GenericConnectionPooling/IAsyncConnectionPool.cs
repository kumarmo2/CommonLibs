using System.Threading.Tasks;

namespace Core.CommonLibs.GenericConnectionPooling
{
    public interface IAsyncConnectionPool<T>
    where T : class
    {

        ValueTask<IConnectionBox<T>> GetConnection();
    }
}