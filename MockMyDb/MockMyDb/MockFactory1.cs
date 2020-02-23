using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Data.SqlClient;

namespace MockMyDb
{
    public abstract class MockFactory : IDisposable
    {
        public string MockDbConnectionString { get {
                if (!deployed)
                    throw new MockException();
                return MockDbConnectionString;
            } protected set; }
        public string MockDatabaseName { get; protected set; }
        private bool deployed = false;

        internal MockFactory(IDbConnection realConnection)
        {
            SetupMockConnection(realConnection);
        }
        protected void SetupMockConnection(IDbConnection realConnection)
        {
            var connection = realConnection;
            MockDatabaseName = GenerateMockDatabaseName(realConnection);
            connection.ChangeDatabase(MockDatabaseName);
            MockDbConnectionString = connection.ConnectionString;
            CreateDatabase(realConnection);
            deployed = true;
        }
        public abstract void Dispose();

        protected string GenerateMockDatabaseName(IDbConnection dbConnection)
        {
            return $"MockDatabase{dbConnection.Database}{DateTime.UtcNow.Ticks}";
        }
        protected abstract void CreateDatabase(IDbConnection insertConnection);
        public abstract IDbConnection GetMockedConnection();
    }

    public class SqlServerMockFactory : MockFactory
    {
        public SqlServerMockFactory(SqlConnection sqlConnection): base(sqlConnection)
        {

        }

        public override void Dispose()
        {
            using (var sqlConnection = new SqlConnection(MockDbConnectionString))
            {
                sqlConnection.Open();
                using (var command = sqlConnection.CreateCommand())
                {
                    command.CommandText = $"Drop Database {MockDatabaseName};";
                    command.ExecuteNonQuery();
                }
            }
        }

        public override IDbConnection GetMockedConnection()
        {
            throw new NotImplementedException();
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
        }
    }

    public class MockFactory<TContext> : MockFactory where TContext : DbContext
    {
        private TContext SetupContext { get; }
        public MockFactory(TContext context) : base(context.Database.GetDbConnection())
        {

        }

        private void SetupDatabase()
        {
            SetupContext.Database.EnsureCreated();
        }

        public override void Dispose()
        {
            SetupContext.Database.EnsureDeleted();
        }
    }
}