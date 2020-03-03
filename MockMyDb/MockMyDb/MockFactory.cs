using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MockMyDb
{
    internal abstract class MockFactory : IMockFactory
    {
        protected string _mockDbConntectionString;
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
        protected string RealConnectionString { get; }
        protected string RealDatabaseName { get; }
        protected bool databaseDeployed = false;

        public MockFactory(IDbConnection dbConnection)
        {
            RealConnectionString = dbConnection.ConnectionString;
            RealDatabaseName = dbConnection.Database;
            SetupMockConnection(dbConnection);
        }
        protected virtual string GenerateMockDatabaseName(IDbConnection dbConnection)
        {
            return $"MockDatabase{dbConnection.Database}{DateTime.UtcNow.Ticks}";
        }
        protected void SetupMockConnection(IDbConnection originalConnection)
        {
            MockDatabaseName = GenerateMockDatabaseName(originalConnection);
            CreateDatabase(originalConnection);
            MockDbConnectionString = GenerateMockConnectionString(originalConnection);
            databaseDeployed = true;
            SetupDatabaseObjects(originalConnection);
        }
        public abstract IDbConnection GetMockConnection();

        protected abstract string GenerateMockConnectionString(IDbConnection originalConnection);

        protected abstract void CreateDatabase(IDbConnection originalConnection);

        protected abstract void SetupDatabaseObjects(IDbConnection orginalConnection);

        public abstract void Dispose();
    }
}
