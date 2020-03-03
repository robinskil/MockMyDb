using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MockMyDb.PostgreSql
{
    internal static class PostgreQueries
    {
        internal static List<string> GetAllTableNames(this NpgsqlConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT table_name
                                        FROM information_schema.tables
                                        WHERE table_schema='public'
                                        AND table_type='BASE TABLE';";
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        var tables = new List<string>();
                        while (reader.Read())
                        {
                            tables.Add(reader.GetString(0));
                        }
                        return tables;
                    }
                    return null;
                }
            }
        }
        internal static string GetTableCreateStatement(this NpgsqlConnection connection, string tableName)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $@"SELECT 'CREATE TABLE ' || '{tableName}' || ' (' || E'\n' || '' || 
                                        string_agg(column_list.column_expr, ', ' || E'\n' || '') || 
                                        '' || E'\n' || ');'
                                        FROM (
                                        SELECT '    ' || column_name || ' ' || data_type || 
                                           coalesce('(' || character_maximum_length || ')', '') || 
                                           case when is_nullable = 'YES' then '' else ' NOT NULL' end as column_expr
                                        FROM information_schema.columns
                                        WHERE table_schema = 'public' AND table_name = '{tableName}'
                                        ORDER BY ordinal_position) column_list;";
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        return reader.GetString(0);
                    }
                    return null;
                }
            }
        }
        internal static PrimaryKey GetPrimaryKey(this NpgsqlConnection connection, string tableName)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText =   $@" SELECT c.column_name, c.data_type
                                            FROM information_schema.table_constraints tc 
                                            JOIN information_schema.constraint_column_usage AS ccu USING (constraint_schema, constraint_name) 
                                            JOIN information_schema.columns AS c ON c.table_schema = tc.constraint_schema
                                              AND tc.table_name = c.table_name AND ccu.column_name = c.column_name
                                            WHERE constraint_type = 'PRIMARY KEY' and tc.table_name = '{tableName}';";
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        PrimaryKey primaryKey = new PrimaryKey(tableName);
                        while (reader.Read())
                        {
                            primaryKey.ColumnNames.Add(reader.GetString(0));
                        }
                        return primaryKey;
                    }
                    return null;
                }
            }
        }
        internal static ICollection<ForeignKey> GetForeignKeys(this NpgsqlConnection connection, string tableName)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT
                                            tc.constraint_name, 
                                            tc.table_name, 
                                            kcu.column_name, 
                                            ccu.table_name AS foreign_table_name,
                                            ccu.column_name AS foreign_column_name 
                                        FROM 
                                            information_schema.table_constraints AS tc 
                                            JOIN information_schema.key_column_usage AS kcu
                                              ON tc.constraint_name = kcu.constraint_name
                                              AND tc.table_schema = kcu.table_schema
                                            JOIN information_schema.constraint_column_usage AS ccu
                                              ON ccu.constraint_name = tc.constraint_name
                                              AND ccu.table_schema = tc.table_schema
                                        WHERE tc.constraint_type = 'FOREIGN KEY' AND tc.table_name='@tablename';";
                command.Parameters.Add("tableName", NpgsqlTypes.NpgsqlDbType.Text);
                command.Parameters["tableName"].Value = tableName;
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        var foreignKeys = new List<ForeignKey>();
                        while (reader.Read())
                        {
                            ForeignKey foreignKey = new ForeignKey(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4));
                            foreignKeys.Add(foreignKey);
                        }
                        return foreignKeys;
                    }
                    return null;
                }
            }
        }
        internal static void CreateTable(this NpgsqlConnection connection , IEnumerable<string> createTableStatements)
        {
            foreach (var createTableStatement in createTableStatements)
            {
                using (var command = connection.CreateCommand())
                {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                    command.CommandText = createTableStatement;
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    int ret = command.ExecuteNonQuery();
                    if(ret != -1)
                    {
                        throw new MockException("Failed to create table.");
                    }
                }
            }
        }
        internal static void CreatePrimaryKey(this NpgsqlConnection connection, IEnumerable<PrimaryKey> primaryKeys)
        {
            foreach (var primaryKey in primaryKeys)
            {
                //Builds the query by adding primary key columns
                var queryBuilder = new StringBuilder();
                queryBuilder.AppendLine($@"ALTER TABLE {primaryKey.TableName} ADD PRIMARY KEY ");
                using (var command = connection.CreateCommand())
                {
                    for (int i = 0; i < primaryKey.ColumnNames.Count; i++)
                    {
                        if(i == 0)
                        {
                            queryBuilder.Append("(");
                        }
                        //Continually add parameters
                        queryBuilder.Append($"{primaryKey.ColumnNames.ElementAt(i)}");
                        if (i == primaryKey.ColumnNames.Count - 1)
                        {
                            queryBuilder.Append(");");
                        }
                        else
                        {
                            queryBuilder.Append(",");
                        }
                    }
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                    command.CommandText = queryBuilder.ToString();
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    int ret = command.ExecuteNonQuery();
                    if (ret != -1)
                    {
                        throw new MockException($"Could not create primary key for {primaryKey.TableName}.");
                    }
                }
            }
        }
        internal static void CreateForeignKeys(this NpgsqlConnection connection, IEnumerable<IEnumerable<ForeignKey>> foreignKeys)
        {
            foreach (var tableForeignKeys in foreignKeys)
            {
                if(tableForeignKeys != null)
                {
                    foreach (var foreignKey in tableForeignKeys)
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = $@"ALTER TABLE {foreignKey.OriginTable} ADD CONSTRAINT {foreignKey.ConstraintName} FOREIGN KEY 
                                                    ({foreignKey.OriginColumnName}) REFERENCES {foreignKey.ReferencedTable} ({foreignKey.ReferencedColumn});";
                            if (command.ExecuteNonQuery() != 1)
                            {
                                throw new MockException($"Could not create primary key for {foreignKey.OriginTable}.");
                            }
                        }
                    }
                }
            }
        }
    }
}
