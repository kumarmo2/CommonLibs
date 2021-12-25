namespace CommonLibs.Utils.Id;
using Snowflake.Core;

public class IdFactory : IIdFactory
{
    // ideally it should be initialized using machine/worker parameters
    private static IdWorker _idWorker = new IdWorker(1, 1);
    public long Next()
    {
        return _idWorker.NextId();
    }
}


