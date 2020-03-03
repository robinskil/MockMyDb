using Microsoft.EntityFrameworkCore;
using MockMyDbTests;
using System;

namespace PostgreSqlTestDatabaseSetUp
{
    class Program
    {
        public const string connectionString = @"Server=127.0.0.1;Port=5432;Database=TestContext;User Id=postgres;Password=";
        static void Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TestContext>();
            optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly("PostgreSqlTestDatabaseSetUp"));
            var context = new TestContext(optionsBuilder.Options);
            context.Database.EnsureCreated();
        }
    }
}
