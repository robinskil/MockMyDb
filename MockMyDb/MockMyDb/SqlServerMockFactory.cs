using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Reflection;

namespace MockMyDb
{
    internal class SqlServerMockFactory : ISqlServerMockFactory
    {
        private string _mockDbConntectionString;
        public string MockDbConnectionString
        {
            get
            {
                if (!databaseDeployed)
                    throw new MockException("Database hasn't been deployed yet.");
                return _mockDbConntectionString;
            }
            protected set => _mockDbConntectionString = value;
        }
        public string MockDatabaseName { get; protected set; }
        private bool databaseDeployed = false;
        public SqlServerMockFactory(SqlConnection sqlConnection)
        {
            SetupMockConnection(sqlConnection);
        }

        public virtual void Dispose()
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(MockDbConnectionString);
            connectionStringBuilder.InitialCatalog = "";
            var serverConnection = connectionStringBuilder.ToString();
            using (var sqlConnection = new SqlConnection(serverConnection))
            {
                sqlConnection.Open();
                using (var command = sqlConnection.CreateCommand())
                {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                    command.CommandText = $"Drop Database {MockDatabaseName};";
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    command.ExecuteNonQuery();
                }
            }
        }
        protected void SetupMockConnection(SqlConnection realConnection)
        {
            MockDatabaseName = GenerateMockDatabaseName(realConnection);
            CreateDatabase(realConnection);
            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder(realConnection.ConnectionString);
            connectionStringBuilder.InitialCatalog = MockDatabaseName;
            MockDbConnectionString = connectionStringBuilder.ToString();
            databaseDeployed = true;
            SetupDatabaseObjects(realConnection);
        }
        protected string GenerateMockDatabaseName(SqlConnection dbConnection)
        {
            return $"MockDatabase{dbConnection.Database}{DateTime.UtcNow.Ticks}";
        }
        protected virtual void SetupDatabaseObjects(SqlConnection realConnection)
        {
            List<string> tableNames = new List<string>();
            List<string> tableCreateStatements = new List<string>();
            List<string> foreignKeyCreateStatements = new List<string>();
            using (var connectionReal = new SqlConnection(realConnection.ConnectionString))
            {
                connectionReal.Open();
                using (var command = connectionReal.CreateCommand())
                {
                    tableNames.AddRange(command.QueryAllTables(realConnection.Database));
                }
                using (var command = connectionReal.CreateCommand())
                {
                    foreach (var tableName in tableNames)
                    {
                        tableCreateStatements.Add(command.QueryTableCreateStatement(tableName));
                    }
                }
                using (var command = connectionReal.CreateCommand())
                {
                    foreignKeyCreateStatements.AddRange(command.QueryAllForeignKeys());
                }
            }
            using (var mockConnection = new SqlConnection(MockDbConnectionString))
            {
                mockConnection.Open();
                using (var command = mockConnection.CreateCommand())
                {
                    foreach (var tableCreateStatement in tableCreateStatements)
                    {
                        command.CommandText = tableCreateStatement;
                        command.ExecuteNonQuery();
                    }
                    //foreach (var foreignKeyCreateStatement in foreignKeyCreateStatements)
                    //{
                    //    command.CommandText = foreignKeyCreateStatement;
                    //    command.ExecuteNonQuery();
                    //}
                }
            }
        }

        public virtual SqlConnection GetMockConnection()
        {
            return new SqlConnection(MockDbConnectionString);
        }

        protected virtual void CreateDatabase(SqlConnection insertConnection)
        {
            using (var connection = new SqlConnection(insertConnection.ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                    command.CommandText = $"CREATE DATABASE {MockDatabaseName}";
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
    }

    internal class SqlServerMockFactory<TContext> : SqlServerMockFactory, ISqlServerMockFactory<TContext> where TContext : DbContext
    {
        public SqlServerMockFactory(string connectionString) : base(new SqlConnection(connectionString))
        {
        }

        public SqlServerMockFactory(TContext context) : base(new SqlConnection(context.Database.GetDbConnection().ConnectionString))
        {

        }

        public TContext CreateMockContext()
        {
            Type[] paramType = new[]
            {
                typeof(DbContextOptions<TContext>)
            };
            var constructor = typeof(TContext).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, paramType, null);
            if (constructor != null)
            {
                var builder = new DbContextOptionsBuilder<TContext>();
                builder.UseSqlServer(MockDbConnectionString);
                var context = constructor.Invoke(new[] { builder.Options });
                return context as TContext;
            }
            throw new MockException($"Couldn't create a mock context, make sure the constructor of the context take a DbContextOptions<{nameof(TContext)}> as a parameter.");
        }

        protected override void SetupDatabaseObjects(SqlConnection realConnection)
        {
            var setupContext = CreateMockContext();
            setupContext.Database.EnsureCreated();
        }

        public override void Dispose()
        {
            var setupContext = CreateMockContext();
            setupContext.Database.EnsureDeleted();
        }
    }
}
