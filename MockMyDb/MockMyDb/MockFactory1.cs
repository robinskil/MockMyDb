using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading;

namespace MockMyDb
{
    public abstract class MockFactory : IDisposable
    {
        private string _mockDbConntectionString;
        public string MockDbConnectionString
        {
            get
            {
                if (!deployed)
                    throw new MockException();
                return _mockDbConntectionString;
            }
            protected set => _mockDbConntectionString = value;
        }
        public string MockDatabaseName { get; protected set; }
        private bool deployed = false;

        internal MockFactory(IDbConnection realConnection)
        {
            SetupMockConnection(realConnection);
        }
        protected void SetupMockConnection(IDbConnection realConnection)
        {
            using (var connection = new SqlConnection(realConnection.ConnectionString))
            {
                MockDatabaseName = GenerateMockDatabaseName(realConnection);

                CreateDatabase(realConnection);
                connection.Open();
                connection.ChangeDatabase(MockDatabaseName);
                MockDbConnectionString = connection.ConnectionString;
            }
            deployed = true;
            SetupDatabaseObjects(realConnection);
        }
        public abstract void Dispose();

        protected string GenerateMockDatabaseName(IDbConnection dbConnection)
        {
            return $"MockDatabase{dbConnection.Database}{DateTime.UtcNow.Ticks}";
        }
        protected abstract void CreateDatabase(IDbConnection insertConnection);
        protected abstract void SetupDatabaseObjects(IDbConnection realConnection);
        public abstract IDbConnection GetMockedConnection();
    }

    public class SqlServerMockFactory : IDisposable
    {
        public SqlServerMockFactory(SqlConnection sqlConnection) : base(sqlConnection)
        {

        }

        public void Dispose()
        {
            using (var sqlConnection = new SqlConnection(MockDbConnectionString))
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

        protected override void SetupDatabaseObjects(IDbConnection realConnection)
        {

        }

        public override IDbConnection GetMockedConnection()
        {
            return new SqlConnection(MockDbConnectionString);
        }

        protected override void CreateDatabase(IDbConnection insertConnection)
        {
            var connection = insertConnection;
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

    public class SqlMockContextFactory<TContext> : SqlServerMockFactory where TContext : DbContext
    {
        private TContext SetupContext { get; set; }
        public SqlMockContextFactory(TContext context) : base(new SqlConnection(context.Database.GetDbConnection().ConnectionString))
        {
        }

        protected override void SetupDatabaseObjects(IDbConnection realConnection)
        {
            if(SetupContext == null)
            {
                SetupContext = CreateContext();
            }
            Thread.Sleep(5000);
            SetupContext.Database.EnsureCreated();
        }

        public override void Dispose()
        {
            SetupContext.Database.EnsureDeleted();
        }

        public TContext CreateContext()
        {
            Type[] paramType = new[]
            {
                typeof(DbContextOptions<TContext>)
            };
            var constructor = typeof(TContext).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, paramType, null);
            if(constructor != null)
            {
                var builder = new DbContextOptionsBuilder<TContext>();
                builder.UseSqlServer(MockDbConnectionString);
                var context =  constructor.Invoke(new[] { builder.Options });
                return context as TContext;
            }
            throw new MockException();
        }

        public override IDbConnection GetMockedConnection()
        {
            return new SqlConnection(MockDbConnectionString);
        }
    }
}