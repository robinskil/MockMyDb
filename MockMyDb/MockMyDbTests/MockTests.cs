using System;
using Xunit;
using MockMyDb;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;

namespace MockMyDbTests
{
    public class MockTests
    {
        public const string connectionString = @"Server=DESKTOP-MNMDILM\TASKAPP;Database=QueryAggregator;Integrated Security=true;";
        [Fact]
        public void CreateDatabase()
        {
            var optionsBuilder = new DbContextOptionsBuilder<QueryTestContext>();
            optionsBuilder.UseSqlServer(connectionString);
            var context = new QueryTestContext(optionsBuilder.Options);
            using (var mock = new DbMock<QueryTestContext>(context,a => new QueryTestContext(a)))
            {
            }
        }
        [Fact]
        public void AddMockDataTest()
        {
            var optionsBuilder = new DbContextOptionsBuilder<QueryTestContext>();
            optionsBuilder.UseSqlServer(connectionString);
            var context = new QueryTestContext(optionsBuilder.Options);
            using (var mock = new DbMock<QueryTestContext>(context, a => new QueryTestContext(a)))
            {
                Guid studentId = Guid.NewGuid();
                mock.MockContext.Students.Add(new Student()
                {
                    Name = "Robin",
                    StudentId = studentId
                });
                mock.MockContext.SaveChanges();
                Assert.NotNull(mock.MockContext.Students.FirstOrDefault(s => s.StudentId == studentId));
            }
        }
    }
}
