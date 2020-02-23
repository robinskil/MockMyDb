using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MockMyDb
{
    public abstract class MockContext : DbContext
    {
        public override void Dispose()
        {
            base.Dispose();
            Database.EnsureDeleted();
        }
    }
}
