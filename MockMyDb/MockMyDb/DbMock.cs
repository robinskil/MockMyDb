using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace MockMyDb
{
    public sealed class DbMock<T> : IDisposable where T : DbContext
    {
        public T MockContext { get; }
        public T OriginalContext { get; }
        public DbMock(T context,Func<DbContextOptions<T>,T> createContext)
        {
            OriginalContext = context;
            var mockConnection = CreateDatabase(context);
            var optionsBuilder = new DbContextOptionsBuilder<T>();
            optionsBuilder.UseSqlServer(mockConnection);
            MockContext = createContext(optionsBuilder.Options);
            //Create the database through a migration
            CreateTables(context.Database.GenerateCreateScript());
        }
        public string GenerateCreateScriptUsed()
        {
            return OriginalContext.Database.GenerateCreateScript();
        }

        private DbConnection CreateDatabase(DbContext context)
        {
            var databaseName = $"MockDatabase{context.Database.GetDbConnection().Database}{DateTime.UtcNow.Ticks}";
            var connection = OriginalContext.Database.GetDbConnection();
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

        private void CreateTables(string createScript)
        {
            OriginalContext.Database.EnsureCreated();
        }

        public void Dispose()
        {
            MockContext.Database.EnsureDeleted();
        }
    }
}
