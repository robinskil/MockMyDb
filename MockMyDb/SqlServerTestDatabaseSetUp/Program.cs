using Microsoft.EntityFrameworkCore;
using MockMyDb;
using MockMyDbTests;
using System;

namespace SqlServerTestDatabaseSetUp
{
    class Program
    {
        public const string connectionString = @"Server=DESKTOP-MNMDILM\TASKAPP;Database=TestContext;Integrated Security=true;";
        static void Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TestContext>();
            optionsBuilder.UseSqlServer(connectionString);
            var context = new TestContext(optionsBuilder.Options);
            context.Database.EnsureCreated();
        }
    }
}
