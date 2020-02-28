using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MockMyDb
{
    public static class Mock
    {
        public static SqlMockContextFactory<TContext> CreateMockFactory<TContext>(TContext context) where TContext : DbContext
        {
            return new SqlMockContextFactory<TContext>(context);
        }
    }
}
