using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace MockMyDb
{
    internal static class SqlServerQueries
    {
        public static string QueryTableCreateStatement(this SqlCommand sqlCommand, string tableName)
        {
            sqlCommand.CommandText = @"DECLARE  
      @object_name SYSNAME  
    , @object_id INT  
    , @table_name NVARCHAR(MAX) 

SELECT @table_name = @tableName  

SELECT  
      @object_name = '[' + OBJECT_SCHEMA_NAME(o.[object_id]) + '].[' + OBJECT_NAME([object_id]) + ']'  
    , @object_id = [object_id]  
FROM (SELECT [object_id] = OBJECT_ID(@table_name, 'U')) o  
  
SELECT 'CREATE TABLE ' + @object_name + CHAR(13) + '(' + CHAR(13) + STUFF((  
    SELECT CHAR(13) + '    , [' + c.name + '] ' +   
        CASE WHEN c.is_computed = 1  
            THEN 'AS ' + OBJECT_DEFINITION(c.[object_id], c.column_id)  
            ELSE   
                CASE WHEN c.system_type_id != c.user_type_id   
                    THEN '[' + SCHEMA_NAME(tp.[schema_id]) + '].[' + tp.name + ']'   
                    ELSE '[' + UPPER(tp.name) + ']'   
                END  +   
                CASE   
                    WHEN tp.name IN ('varchar', 'char', 'varbinary', 'binary')  
                        THEN '(' + CASE WHEN c.max_length = -1   
                                        THEN 'MAX'   
                                        ELSE CAST(c.max_length AS VARCHAR(5))   
                                    END + ')'  
                    WHEN tp.name IN ('nvarchar', 'nchar')  
                        THEN '(' + CASE WHEN c.max_length = -1   
                                        THEN 'MAX'   
                                        ELSE CAST(c.max_length / 2 AS VARCHAR(5))   
                                    END + ')'  
                    WHEN tp.name IN ('datetime2', 'time2', 'datetimeoffset')   
                        THEN '(' + CAST(c.scale AS VARCHAR(5)) + ')'  
                    WHEN tp.name = 'decimal'  
                        THEN '(' + CAST(c.[precision] AS VARCHAR(5)) + ',' + CAST(c.scale AS VARCHAR(5)) + ')'  
                    ELSE ''  
                END +  
                CASE WHEN c.collation_name IS NOT NULL AND c.system_type_id = c.user_type_id   
                    THEN ' COLLATE ' + c.collation_name  
                    ELSE ''  
                END +  
                CASE WHEN c.is_nullable = 1   
                    THEN ' NULL'  
                    ELSE ' NOT NULL'  
                END +  
                CASE WHEN c.default_object_id != 0   
                    THEN ' CONSTRAINT [' + OBJECT_NAME(c.default_object_id) + ']' +   
                         ' DEFAULT ' + OBJECT_DEFINITION(c.default_object_id)  
                    ELSE ''  
                END +   
                CASE WHEN cc.[object_id] IS NOT NULL   
                    THEN ' CONSTRAINT [' + cc.name + '] CHECK ' + cc.[definition]  
                    ELSE ''  
                END +  
                CASE WHEN c.is_identity = 1   
                    THEN ' IDENTITY(' + CAST(IDENTITYPROPERTY(c.[object_id], 'SeedValue') AS VARCHAR(5)) + ',' +   
                                    CAST(IDENTITYPROPERTY(c.[object_id], 'IncrementValue') AS VARCHAR(5)) + ')'   
                    ELSE ''   
                END   
        END  
    FROM sys.columns c WITH(NOLOCK)  
    JOIN sys.types tp WITH(NOLOCK) ON c.user_type_id = tp.user_type_id  
    LEFT JOIN sys.check_constraints cc WITH(NOLOCK)   
         ON c.[object_id] = cc.parent_object_id   
        AND cc.parent_column_id = c.column_id  
    WHERE c.[object_id] = @object_id  
    ORDER BY c.column_id  
    FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 7, '      ') +   
    ISNULL((SELECT '  
    , CONSTRAINT [' + i.name + '] PRIMARY KEY ' +   
    CASE WHEN i.index_id = 1   
        THEN 'CLUSTERED'   
        ELSE 'NONCLUSTERED'   
    END +' (' + (  
    SELECT STUFF(CAST((  
        SELECT ', [' + COL_NAME(ic.[object_id], ic.column_id) + ']' +  
                CASE WHEN ic.is_descending_key = 1  
                    THEN ' DESC'  
                    ELSE ''  
                END  
        FROM sys.index_columns ic WITH(NOLOCK)  
        WHERE i.[object_id] = ic.[object_id]  
            AND i.index_id = ic.index_id  
        FOR XML PATH(N''), TYPE) AS NVARCHAR(MAX)), 1, 2, '')) + ')'  
    FROM sys.indexes i WITH(NOLOCK)  
    WHERE i.[object_id] = @object_id  
        AND i.is_primary_key = 1), '') + CHAR(13) + ');'  ";
            sqlCommand.Parameters.Add("@tableName", System.Data.SqlDbType.NVarChar);
            sqlCommand.Parameters["@tableName"].Value = tableName;
            var reader = sqlCommand.ExecuteReader();
            sqlCommand.Parameters.Clear();
            if (reader.Read())
            {
                string result = reader.GetString(0);
                reader.Close();
                return result;
            }
            reader.Close();
            return null;
        }

        public static IEnumerable<string> QueryAllTables(this SqlCommand sqlCommand, string dbName)
        {
            sqlCommand.CommandText = @"SELECT TABLE_NAME
                                        FROM INFORMATION_SCHEMA.TABLES
                                        WHERE TABLE_TYPE = 'BASE TABLE'";
            var reader = sqlCommand.ExecuteReader();
            var tables = new List<string>();
            while (reader.Read())
            {
                tables.Add(reader.GetString(0));
            }
            reader.Close();
            return tables;
        }

        public static IEnumerable<string> QueryAllForeignKeys(this SqlCommand sqlCommand)
        {
            sqlCommand.CommandText = @"SELECT 
                                       'ALTER TABLE [' + OBJECT_NAME(f.parent_object_id) + ']  WITH NOCHECK ADD CONSTRAINT [' + 
                                            f.name + '] FOREIGN KEY([' + COL_NAME(fc.parent_object_id,fc.parent_column_id) + ']) REFERENCES ' + 
                                            '[' + OBJECT_NAME(t.object_id) + '] ([' +
                                            COL_NAME(t.object_id,fc.referenced_column_id) + '])' AS 'Create foreign key'
                                        FROM sys.foreign_keys AS f,
                                             sys.foreign_key_columns AS fc,
                                             sys.tables t 
                                        WHERE f.OBJECT_ID = fc.constraint_object_id
                                        AND t.OBJECT_ID = fc.referenced_object_id";
            var reader = sqlCommand.ExecuteReader();
            var foreignKeyStatements = new List<string>();
            while (reader.Read())
            {
                foreignKeyStatements.Add(reader.GetString(0));
            }
            reader.Close();
            return foreignKeyStatements;
        }
    }
}
