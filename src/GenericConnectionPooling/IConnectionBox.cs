using System;

namespace Core.CommonLibs.GenericConnectionPooling
{
    public interface IConnectionBox<T> : IAsyncDisposable
    where T : class
    {
        public T Connection { get; }
    }
}