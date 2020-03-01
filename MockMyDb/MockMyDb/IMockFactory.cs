using System;
using System.Data;

namespace MockMyDb
{
    public interface IMockFactory : IDisposable
    {
        string MockDatabaseName { get; }
        string MockDbConnectionString { get; }
        IDbConnection GetMockConnection();
    }
}