using Microsoft.EntityFrameworkCore;
using MockMyDb.PostgreSql;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MockMyDb
{
    internal class PostgreSqlMockFactory : MockFactory
    {
        public PostgreSqlMockFactory(NpgsqlConnection dbConnection) : base(dbConnection)
        {
        }

        public override void Dispose()
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(MockDbConnectionString);
            connectionStringBuilder.Database = RealDatabaseName;
            var serverConnection = connectionStringBuilder.ToString();
            using (var connection = new NpgsqlConnection(serverConnection))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                    command.CommandText = $@"SELECT * FROM pg_stat_activity WHERE datname = '{MockDatabaseName}';
                                            DROP DATABASE {MockDatabaseName}";
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    command.ExecuteNonQuery();
                }
            }
        }

        public override IDbConnection GetMockConnection()
        {
            return GetNpgsqlConnection();
        }

        public NpgsqlConnection GetNpgsqlConnection()
        {
            return new NpgsqlConnection(MockDbConnectionString);
        }

        protected override void CreateDatabase(IDbConnection originalConnection)
        {
            using (var connection = new NpgsqlConnection(originalConnection.ConnectionString))
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

        protected override string GenerateMockConnectionString(IDbConnection originalConnection)
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(originalConnection.ConnectionString);
            connectionStringBuilder.Database = MockDatabaseName;
            return connectionStringBuilder.ToString();
        }

        protected override void SetupDatabaseObjects(IDbConnection orginalConnection)
        {
            List<string> tableCreateQueries;
            List<PrimaryKey> primaryKeys;
            List<ICollection<ForeignKey>> foreignKeys;
            using (var connection = new NpgsqlConnection(orginalConnection.ConnectionString))
            {
                connection.Open();
                var tables = connection.GetAllTableNames();
                tableCreateQueries = tables.Select(connection.GetTableCreateStatement).ToList();
                primaryKeys = tables.Select(connection.GetPrimaryKey).ToList();
                foreignKeys = tables.Select(connection.GetForeignKeys).ToList();
            }
            using (var connection = new NpgsqlConnection(MockDbConnectionString))
            {
                connection.Open();
                if (tableCreateQueries != null)
                {
                    connection.CreateTable(tableCreateQueries);
                }
                if (primaryKeys != null)
                {
                    connection.CreatePrimaryKey(primaryKeys);
                }
                if(foreignKeys != null)
                {
                    connection.CreateForeignKeys(foreignKeys);
                }
            }
        }
    }

    internal class PostgreSqlMockFactory<TContext> : PostgreSqlMockFactory where TContext : DbContext
    {
        public PostgreSqlMockFactory(TContext context) : base(new NpgsqlConnection(context.Database.GetDbConnection().ConnectionString))
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
                builder.UseNpgsql(MockDbConnectionString);
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
