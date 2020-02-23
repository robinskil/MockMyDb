using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace MockMyDb
{
    public static class MockFactory
    {
        public static TContext CreateSqlServerMockContext<TContext>(DbContext contextBase, Func<DbContextOptions<TContext>, TContext> createContext) where TContext : DbContext
        {
            var connection = CreateSqlServerDatabase(contextBase.Database.GetDbConnection());
            var mockContext = SetupContextContainer<TContext>(connection, createContext);
            return mockContext.GetMockedContext();
        }
        public static TContext CreateSqlServerMockContext<TContext>(SqlConnection sqlConnection, Func<DbContextOptions<TContext>, TContext> createContext) where TContext : DbContext
        {
            var connection = CreateSqlServerDatabase(sqlConnection);
            var mockContext = SetupContextContainer<TContext>(sqlConnection,createContext);
            return mockContext.GetMockedContext();
        }
        private static MockContextContainer<TContext> SetupContextContainer<TContext>(DbConnection connection, Func<DbContextOptions<TContext>, TContext> createContext) where TContext : DbContext
        {
            var optionsBuilder = new DbContextOptionsBuilder<TContext>();
            optionsBuilder.UseSqlServer(connection);
            return new MockContextContainer<TContext>(createContext(optionsBuilder.Options));
        }
        private static DbConnection CreateSqlServerDatabase(DbConnection insertConnection)
        {
            var databaseName = $"MockDatabase{insertConnection.Database}{DateTime.UtcNow.Ticks}";
            var connection = insertConnection;
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
