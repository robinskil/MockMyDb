using Microsoft.EntityFrameworkCore;
using System;
using System.Data.SqlClient;

namespace MockMyDb
{
    public class DbMock<T> : IDisposable where T : DbContext , new()
    {
        public T Context { get; }
        public DbMock(DbContext context,Func<DbContextOptions<T>,T> createContext)
        {
            var connection = context.Database.GetDbConnection();
            var optionsBuilder = new DbContextOptionsBuilder<T>();
            connection.ChangeDatabase($"MockDatabase-{Guid.NewGuid()}");
            optionsBuilder.UseSqlServer(connection);
            Context = createContext(optionsBuilder.Options);
            //Create the database through a migration
            CreateDatabase(context.Database.GenerateCreateScript());
        }

        private void CreateDatabase(string createScript)
        {
            var connection = Context.Database.GetDbConnection();
            connection.Open();
            using (var command = connection.CreateCommand())
            {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                command.CommandText = createScript;
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                command.ExecuteNonQuery();
            }
        }

        public void Dispose()
        {
            Context.Database.EnsureDeleted();
        }
    }
}
