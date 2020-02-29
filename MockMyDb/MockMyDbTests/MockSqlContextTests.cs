using System;
using Xunit;
using MockMyDb;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;

namespace MockMyDbTests
{
    public class MockSqlContextTests
    {
        public const string connectionString = @"Server=DESKTOP-MNMDILM\TASKAPP;Database=QueryAggregator;Integrated Security=true;";
        [Fact]
        public void CreateDatabaseContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<TestContext>();
            optionsBuilder.UseSqlServer(connectionString);
            var context = new TestContext(optionsBuilder.Options);
            using (var mockFactory = Mock.CreateMockFactory(context))
            {
                var context2 = mockFactory.CreateMockContext();
            }
        }
        [Fact]
        public void CreateDatabaseConnectionString()
        {
            using (var mockFactory = Mock.CreateMockFactory<TestContext>(connectionString))
            {
                var context2 = mockFactory.CreateMockContext();
            }
        }
        [Fact]
        public void AddMockDataTest()
        {
            var optionsBuilder = new DbContextOptionsBuilder<TestContext>();
            optionsBuilder.UseSqlServer(connectionString);
            var context = new TestContext(optionsBuilder.Options);
            using (var mock = Mock.CreateMockFactory(context))
            {
                var mockContext = mock.CreateMockContext();
                Guid studentId = Guid.NewGuid();
                mockContext.Students.Add(new Student()
                {
                    Name = "Robin",
                    StudentId = studentId
                });
                mockContext.SaveChanges();
                Assert.NotNull(mockContext.Students.FirstOrDefault(s => s.StudentId == studentId));
            }
        }
    }
}
