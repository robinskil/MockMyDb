using System;
using Xunit;
using MockMyDb;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;

namespace MockMyDbTests
{
    public class ContextMockTests
    {
        public const string connectionString = @"Server=DESKTOP-MNMDILM\TASKAPP;Database=QueryAggregator;Integrated Security=true;";
        [Fact]
        public void CreateDatabase()
        {
            var optionsBuilder = new DbContextOptionsBuilder<TestContext>();
            optionsBuilder.UseSqlServer(connectionString);
            var context = new TestContext(optionsBuilder.Options);
            using (var mockFactory = Mock.CreateMockFactory(context))
            {
                //var context2 = mockFactory.CreateContext();
            }
            //using (var mock = MockFactory.CreateSqlServerMockContext<TestContext>(context,a => new TestContext(a)))
            //{
            //}
        }
        [Fact]
        public void AddMockDataTest()
        {
            //var optionsBuilder = new DbContextOptionsBuilder<TestContext>();
            //optionsBuilder.UseSqlServer(connectionString);
            //var context = new TestContext(optionsBuilder.Options);
            //using (var mock = MockFactory.CreateSqlServerMockContext<TestContext>(context, a => new TestContext(a)))
            //{
            //    Guid studentId = Guid.NewGuid();
            //    mock.Students.Add(new Student()
            //    {
            //        Name = "Robin",
            //        StudentId = studentId
            //    });
            //    mock.SaveChanges();
            //    Assert.NotNull(mock.Students.FirstOrDefault(s => s.StudentId == studentId));
            //}
        }
    }
}
