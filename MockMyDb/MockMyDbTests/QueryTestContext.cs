using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MockMyDbTests
{
    public class QueryTestContext : DbContext
    {
        public QueryTestContext(DbContextOptions<QueryTestContext> options) : base(options)
        {
        }
        public DbSet<Student> Students { get; set; }
        public DbSet<Grade> Grades { get; set; }
    }
}
