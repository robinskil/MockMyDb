using Microsoft.Data.SqlClient;
using MockMyDb;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MockMyDbTests
{
    public class MockSqlAdoTests
    {
        public const string connectionString = @"Server=DESKTOP-MNMDILM\TASKAPP;Database=QueryAggregator;Integrated Security=true;";

        [Fact]
        public void CreateDatabaseConnectionObject()
        {
            using (var mockFactory = Mock.CreateMockFactory(new SqlConnection(connectionString)))
            {
                using (var connection = mockFactory.GetMockConnection())
                {
                    connection.Open();
                }
            }
        }
        [Fact]
        public void CreateDatabaseConnectionString()
        {
            using (var mockFactory = Mock.CreateMockFactory(connectionString))
            {
                using (var connection = mockFactory.GetMockConnection())
                {
                    connection.Open();
                }
            }
        }
        [Fact]
        public void AddMockDataTest()
        {
            Student s = new Student()
            {
                StudentId = Guid.NewGuid(),
                Name = "Test"
            };
            using (var mockFactory = Mock.CreateMockFactory(connectionString))
            {
                using (var connection = mockFactory.GetSqlMockConnection())
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "Insert into Students values (@studentId,@name)";
                        var para = command.CreateParameter();
                        command.Parameters.Add("@studentId", System.Data.SqlDbType.UniqueIdentifier);
                        command.Parameters.Add("@name", System.Data.SqlDbType.NVarChar);
                        command.Parameters["@name"].Value = s.Name;
                        command.Parameters["@studentId"].Value = s.StudentId;
                        Assert.Equal(1,command.ExecuteNonQuery());
                    }
                }
            }
        }
    }
}
