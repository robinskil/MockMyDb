using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;

namespace MockMyDb
{
    public interface ISqlServerMockFactory : IMockFactory
    {
        SqlConnection GetSqlMockConnection();
    }
    public interface ISqlServerMockFactory<TContext> : ISqlServerMockFactory where TContext : DbContext
    {
        TContext CreateMockContext();
    }
}