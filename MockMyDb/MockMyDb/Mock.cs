using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MockMyDb
{
    public static class Mock
    {
        public static ISqlServerMockFactory<TContext> CreateMockFactory<TContext>(TContext context) where TContext : DbContext
        {
            return new SqlServerMockFactory<TContext>(context);
        }
        public static ISqlServerMockFactory<TContext> CreateMockFactory<TContext>(string connectionString) where TContext : DbContext
        {
            return new SqlServerMockFactory<TContext>(connectionString);
        }
        public static ISqlServerMockFactory CreateMockFactory(SqlConnection sqlConnection)
        {
            return new SqlServerMockFactory(sqlConnection);
        }
        public static ISqlServerMockFactory CreateMockFactory(string sqlConnection)
        {
            return new SqlServerMockFactory(new SqlConnection(sqlConnection));
        }
    }
}
