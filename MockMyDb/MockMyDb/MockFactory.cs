using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace MockMyDb
{
    public static class MockFactory
    {
        public static TContext CreateMockContext<TContext>(DbContext contextBase, Func<DbContextOptions<TContext>, TContext> createContext) where TContext : MockContext
        {
            var connection = CreateDatabase(contextBase);
            var optionsBuilder = new DbContextOptionsBuilder<TContext>();
            optionsBuilder.UseSqlServer(connection);
            TContext mockContext = createContext(optionsBuilder.Options);
            mockContext.Database.EnsureCreated();
            return mockContext;
        }
        private static DbConnection CreateDatabase(DbContext context)
        {
            var databaseName = $"MockDatabase{context.Database.GetDbConnection().Database}{DateTime.UtcNow.Ticks}";
            var connection = context.Database.GetDbConnection();
            connection.Open();
            using (var command = connection.CreateCommand())
            {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                command.CommandText = $"CREATE DATABASE {databaseName}";
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                command.ExecuteNonQuery();
            }
            connection.ChangeDatabase(databaseName);
            return connection;
        }
    }
}
