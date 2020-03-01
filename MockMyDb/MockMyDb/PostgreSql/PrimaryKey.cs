using System;
using System.Collections.Generic;
using System.Text;

namespace MockMyDb.PostgreSql
{
    internal class PrimaryKey
    {
        public ICollection<string> ColumnNames { get; }
        public string TableName { get; }

        public PrimaryKey(string tableName)
        {
            ColumnNames = new List<string>();
            TableName = tableName;
        }
    }
}
