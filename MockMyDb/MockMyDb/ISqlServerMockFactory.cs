using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;

namespace MockMyDb
{
    public interface ISqlServerMockFactory : IDisposable
    {
        string MockDatabaseName { get; }
        string MockDbConnectionString { get; }
        SqlConnection GetMockedConnection();
    }
    public interface ISqlServerMockFactory<TContext> : ISqlServerMockFactory where TContext : DbContext
    {
        TContext CreateMockContext();
    }
}