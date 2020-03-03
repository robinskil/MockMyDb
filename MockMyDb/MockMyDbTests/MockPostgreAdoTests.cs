using MockMyDb;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MockMyDbTests
{
    public class MockPostgreAdoTests
    {
        public const string connectionString = @"Server=127.0.0.1;Port=5432;Database=TestContext;User Id=postgres;Password=93650f0294fab56a39a24ea76270dc5d";
        [Fact]
        public void CreateDatabaseConnectionObject()
        {
            using (var mockFactory = Mock.CreateMockFactoryPostgres(new NpgsqlConnection(connectionString)))
            {
                using (var connection = mockFactory.GetNpgsqlConnection())
                {
                    connection.Open();
                }
            }
        }
        [Fact]
        public void CreateDatabaseConnectionString()
        {
            using (var mockFactory = Mock.CreateMockFactoryPostgres(connectionString))
            {
                using (var connection = mockFactory.GetNpgsqlConnection())
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
            using (var mockFactory = Mock.CreateMockFactoryPostgres(connectionString))
            {
                using (var connection = mockFactory.GetNpgsqlConnection())
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "Insert into Students values (@studentId,@name)";
                        var para = command.CreateParameter();
                        command.Parameters.Add("@studentId", NpgsqlTypes.NpgsqlDbType.Uuid);
                        command.Parameters.Add("@name", NpgsqlTypes.NpgsqlDbType.Text);
                        command.Parameters["@name"].Value = s.Name;
                        command.Parameters["@studentId"].Value = s.StudentId;
                        Assert.Equal(1, command.ExecuteNonQuery());
                    }
                }
            }
        }
    }
}
