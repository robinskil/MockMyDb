using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MockMyDb
{
    public sealed class MockContextContainer<T> where T : DbContext , IDisposable
    {
        private T Context { get; }
        public MockContextContainer(T context)
        {
            Context = context;
        }
        public T GetMockedContext()
        {
            return Context;
        }
        public void Dispose()
        {
            Context.Database.EnsureDeleted();
        }
    }
}
