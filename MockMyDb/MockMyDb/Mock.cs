using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MockMyDb
{
    public static class Mock
    {
        public static MockFactory<TContext> CreateMockFactory<TContext>() where TContext : DbContext
        {

        }
    }
}
