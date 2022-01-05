using System.Data;

namespace CommonLibs.Database
{
    public interface IDbConnectionFactory
    {
        IDbConnection GetDbConnection();
    }
}