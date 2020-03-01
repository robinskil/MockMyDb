using System;
using System.Collections.Generic;
using System.Text;

namespace MockMyDb.PostgreSql
{
    internal class ForeignKey
    {
        public string ConstraintName { get; }
        public string OriginTable { get; }
        public string OriginColumnName { get; }
        public string ReferencedTable { get; }
        public string ReferencedColumn { get; }
        public ForeignKey(string constraintName, string originTable, string originColumnName, string referencedTable, string referencedColumn)
        {
            ConstraintName = constraintName;
            OriginTable = originTable;
            OriginColumnName = originColumnName;
            ReferencedTable = referencedTable;
            ReferencedColumn = referencedColumn;
        }
    }
}
