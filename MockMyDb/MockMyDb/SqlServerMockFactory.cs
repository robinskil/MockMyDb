using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Reflection;
using System.Data;

namespace MockMyDb
{
    internal class SqlServerMockFactory : MockFactory , ISqlServerMockFactory 
    {
        public SqlServerMockFactory(SqlConnection sqlConnection) : base(sqlConnection)
        {
            SetupMockConnection(sqlConnection);
        }

        public override void Dispose()
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(MockDbConnectionString);
            connectionStringBuilder.InitialCatalog = RealDatabaseName;
            var serverConnection = connectionStringBuilder.ToString();
            using (var sqlConnection = new SqlConnection(serverConnection))
            {
                sqlConnection.Open();
                using (var command = sqlConnection.CreateCommand())
                {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                    command.CommandText = $@"ALTER DATABASE {MockDatabaseName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                                            DROP DATABASE {MockDatabaseName}";
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    command.ExecuteNonQuery();
                }
            }
        }

        public override IDbConnection GetMockConnection()
        {
            return GetSqlMockConnection();
        }

        public SqlConnection GetSqlMockConnection()
        {
            return new SqlConnection(MockDbConnectionString);
        }
        protected override string GenerateMockConnectionString(IDbConnection originalConnection)
        {
            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder(originalConnection.ConnectionString);
            connectionStringBuilder.InitialCatalog = MockDatabaseName;
            return connectionStringBuilder.ToString();
        }

        protected override void CreateDatabase(IDbConnection originalConnection)
        {
            using (var connection = new SqlConnection(originalConnection.ConnectionString))
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

        protected override void SetupDatabaseObjects(IDbConnection orginalConnection)
        {
            List<string> tableNames = new List<string>();
            List<string> tableCreateStatements = new List<string>();
            List<string> foreignKeyCreateStatements = new List<string>();
            using (var connectionReal = new SqlConnection(orginalConnection.ConnectionString))
            {
                connectionReal.Open();
                using (var command = connectionReal.CreateCommand())
                {
                    tableNames.AddRange(command.QueryAllTables(orginalConnection.Database));
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
                    foreach (var foreignKeyCreateStatement in foreignKeyCreateStatements)
                    {
                        command.CommandText = foreignKeyCreateStatement;
                        command.ExecuteNonQuery();
                    }
                }
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

        protected override void SetupDatabaseObjects(IDbConnection realConnection)
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
