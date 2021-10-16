using System;

namespace CommonLibs.GenericConnectionPooling
{
    public interface IConnectionBox<T> : IAsyncDisposable
    where T : class
    {
        public T Connection { get; }
    }
}