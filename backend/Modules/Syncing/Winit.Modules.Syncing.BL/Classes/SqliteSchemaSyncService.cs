using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Npgsql;
using Serilog;
using Dapper;

namespace Winit.Modules.Syncing.BL.Classes
{
    public class SqliteSchemaSyncService
    {
        private readonly string _postgresConnectionString;
        private readonly ILogger _logger = Log.ForContext<SqliteSchemaSyncService>();

        public SqliteSchemaSyncService(string postgresConnectionString)
        {
            _postgresConnectionString = postgresConnectionString;
        }

        /// <summary>
        /// Synchronizes SQLite schema with PostgreSQL schema
        /// </summary>
        public async Task<bool> SynchronizeSchema(string sqliteFilePath, string tableName)
        {
            try
            {
                using var pgConnection = new NpgsqlConnection(_postgresConnectionString);
                using var sqliteConnection = new SqliteConnection($"Data Source={sqliteFilePath}");
                
                await pgConnection.OpenAsync();
                await sqliteConnection.OpenAsync();

                // Check if table exists in SQLite
                var tableExists = await TableExistsInSqlite(sqliteConnection, tableName);
                
                if (!tableExists)
                {
                    _logger.Information("Table {TableName} does not exist in SQLite. Creating it.", tableName);
                    await CreateTableInSqlite(pgConnection, sqliteConnection, tableName);
                }
                else
                {
                    _logger.Information("Table {TableName} exists in SQLite. Checking for missing columns.", tableName);
                    await SyncTableColumns(pgConnection, sqliteConnection, tableName);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error synchronizing schema for table {TableName}", tableName);
                return false;
            }
        }

        /// <summary>
        /// Synchronizes all tables in a list
        /// </summary>
        public async Task<Dictionary<string, bool>> SynchronizeSchemas(string sqliteFilePath, List<string> tableNames)
        {
            var results = new Dictionary<string, bool>();
            
            foreach (var tableName in tableNames)
            {
                var success = await SynchronizeSchema(sqliteFilePath, tableName);
                results[tableName] = success;
            }

            return results;
        }

        private async Task<bool> TableExistsInSqlite(SqliteConnection connection, string tableName)
        {
            var sql = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName";
            var result = await connection.QueryFirstOrDefaultAsync<string>(sql, new { tableName });
            return !string.IsNullOrEmpty(result);
        }

        private async Task CreateTableInSqlite(NpgsqlConnection pgConnection, SqliteConnection sqliteConnection, string tableName)
        {
            // Get PostgreSQL table schema
            var pgColumns = await GetPostgresTableSchema(pgConnection, tableName);
            
            if (!pgColumns.Any())
            {
                _logger.Warning("No columns found for table {TableName} in PostgreSQL", tableName);
                return;
            }

            // Build CREATE TABLE statement
            var createTableSql = BuildCreateTableStatement(tableName, pgColumns);
            
            _logger.Information("Creating table {TableName} in SQLite with SQL: {Sql}", tableName, createTableSql);
            
            // Execute CREATE TABLE
            await sqliteConnection.ExecuteAsync(createTableSql);
            
            // Create indexes for common columns
            await CreateIndexes(sqliteConnection, tableName);
        }

        private async Task SyncTableColumns(NpgsqlConnection pgConnection, SqliteConnection sqliteConnection, string tableName)
        {
            // Get columns from both databases
            var pgColumns = await GetPostgresTableSchema(pgConnection, tableName);
            var sqliteColumns = await GetSqliteTableSchema(sqliteConnection, tableName);

            // Find missing columns
            var missingColumns = pgColumns.Where(pg => !sqliteColumns.Any(sq => sq.ColumnName.Equals(pg.ColumnName, StringComparison.OrdinalIgnoreCase))).ToList();

            if (!missingColumns.Any())
            {
                _logger.Information("No missing columns found for table {TableName}", tableName);
                return;
            }

            // Add missing columns
            foreach (var column in missingColumns)
            {
                var alterTableSql = $"ALTER TABLE {tableName} ADD COLUMN {column.ColumnName} {MapPostgresToSqliteType(column.DataType, column.MaxLength)}";
                
                if (column.HasDefault)
                {
                    alterTableSql += $" DEFAULT {GetDefaultValue(column.DataType, column.DefaultValue)}";
                }
                
                _logger.Information("Adding column {ColumnName} to table {TableName} in SQLite", column.ColumnName, tableName);
                
                try
                {
                    await sqliteConnection.ExecuteAsync(alterTableSql);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error adding column {ColumnName} to table {TableName}", column.ColumnName, tableName);
                }
            }
        }

        private async Task<List<ColumnInfo>> GetPostgresTableSchema(NpgsqlConnection connection, string tableName)
        {
            var sql = @"
                SELECT 
                    column_name AS ColumnName,
                    data_type AS DataType,
                    character_maximum_length AS MaxLength,
                    is_nullable AS IsNullable,
                    column_default AS DefaultValue,
                    CASE WHEN column_default IS NOT NULL THEN true ELSE false END AS HasDefault
                FROM information_schema.columns
                WHERE table_name = @tableName
                AND table_schema = 'public'
                ORDER BY ordinal_position";

            var columns = await connection.QueryAsync<ColumnInfo>(sql, new { tableName });
            return columns.ToList();
        }

        private async Task<List<ColumnInfo>> GetSqliteTableSchema(SqliteConnection connection, string tableName)
        {
            var sql = $"PRAGMA table_info({tableName})";
            var result = await connection.QueryAsync(sql);
            
            return result.Select(r => new ColumnInfo
            {
                ColumnName = r.name,
                DataType = r.type,
                IsNullable = r.notnull == 0 ? "YES" : "NO",
                DefaultValue = r.dflt_value,
                HasDefault = r.dflt_value != null
            }).ToList();
        }

        private string BuildCreateTableStatement(string tableName, List<ColumnInfo> columns)
        {
            var sb = new StringBuilder($"CREATE TABLE IF NOT EXISTS {tableName} (");
            var columnDefinitions = new List<string>();

            foreach (var column in columns)
            {
                var columnDef = $"{column.ColumnName} {MapPostgresToSqliteType(column.DataType, column.MaxLength)}";
                
                if (column.ColumnName.Equals("Id", StringComparison.OrdinalIgnoreCase))
                {
                    columnDef += " PRIMARY KEY AUTOINCREMENT";
                }
                else if (column.ColumnName.Equals("UID", StringComparison.OrdinalIgnoreCase))
                {
                    columnDef += " UNIQUE";
                }
                
                if (column.IsNullable == "NO" && !column.ColumnName.Equals("Id", StringComparison.OrdinalIgnoreCase))
                {
                    columnDef += " NOT NULL";
                }
                
                if (column.HasDefault)
                {
                    columnDef += $" DEFAULT {GetDefaultValue(column.DataType, column.DefaultValue)}";
                }
                
                columnDefinitions.Add(columnDef);
            }

            sb.Append(string.Join(", ", columnDefinitions));
            sb.Append(")");

            return sb.ToString();
        }

        private string MapPostgresToSqliteType(string pgType, long? maxLength)
        {
            return pgType.ToLower() switch
            {
                "integer" or "int" or "int4" => "INTEGER",
                "bigint" or "int8" => "INTEGER",
                "smallint" or "int2" => "INTEGER",
                "numeric" or "decimal" => "REAL",
                "real" or "float4" => "REAL",
                "double precision" or "float8" => "REAL",
                "boolean" or "bool" => "INTEGER",
                "text" => "TEXT",
                "character varying" or "varchar" => maxLength.HasValue ? $"VARCHAR({maxLength})" : "VARCHAR(250)",
                "character" or "char" => maxLength.HasValue ? $"VARCHAR({maxLength})" : "VARCHAR(50)",
                "timestamp without time zone" or "timestamp" => "DATETIME",
                "timestamp with time zone" or "timestamptz" => "DATETIME",
                "date" => "DATE",
                "time" or "time without time zone" => "TIME",
                "uuid" => "VARCHAR(36)",
                "json" or "jsonb" => "TEXT",
                _ => "TEXT"
            };
        }

        private string GetDefaultValue(string dataType, string defaultValue)
        {
            if (string.IsNullOrEmpty(defaultValue))
                return "NULL";

            // Clean up PostgreSQL default values
            defaultValue = defaultValue.Replace("::character varying", "")
                                     .Replace("::text", "")
                                     .Replace("::integer", "")
                                     .Replace("::boolean", "")
                                     .Replace("::timestamp without time zone", "");

            // Handle boolean values
            if (dataType.ToLower() == "boolean" || dataType.ToLower() == "bool")
            {
                if (defaultValue.ToLower().Contains("true"))
                    return "1";
                if (defaultValue.ToLower().Contains("false"))
                    return "0";
            }

            // Handle numeric values
            if (IsNumericType(dataType))
            {
                // Extract numeric value from expressions like 'nextval(...)'
                if (defaultValue.Contains("nextval"))
                    return "NULL"; // SQLite will handle autoincrement
                
                // Try to parse as number
                if (int.TryParse(defaultValue, out _) || decimal.TryParse(defaultValue, out _))
                    return defaultValue;
            }

            // Handle string values
            if (!defaultValue.StartsWith("'") && !defaultValue.EndsWith("'"))
            {
                return $"'{defaultValue}'";
            }

            return defaultValue;
        }

        private bool IsNumericType(string dataType)
        {
            var numericTypes = new[] { "integer", "int", "bigint", "smallint", "numeric", "decimal", "real", "double precision", "float" };
            return numericTypes.Any(t => dataType.ToLower().Contains(t));
        }

        private async Task CreateIndexes(SqliteConnection connection, string tableName)
        {
            // Create common indexes
            var indexColumns = new[] { "UID", "CompanyUID", "OrgUID", "CreatedTime", "ModifiedTime", "ServerModifiedTime", "ss" };
            
            foreach (var column in indexColumns)
            {
                try
                {
                    var indexName = $"idx_{tableName}_{column}";
                    var sql = $"CREATE INDEX IF NOT EXISTS {indexName} ON {tableName} ({column})";
                    await connection.ExecuteAsync(sql);
                }
                catch (Exception ex)
                {
                    // Column might not exist, ignore
                    _logger.Debug(ex, "Could not create index for column {Column} on table {Table}", column, tableName);
                }
            }
        }

        private class ColumnInfo
        {
            public string ColumnName { get; set; }
            public string DataType { get; set; }
            public long? MaxLength { get; set; }
            public string IsNullable { get; set; }
            public string DefaultValue { get; set; }
            public bool HasDefault { get; set; }
        }
    }
}