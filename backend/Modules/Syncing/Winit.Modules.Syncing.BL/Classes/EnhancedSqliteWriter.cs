using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Serilog;
using Dapper;

namespace Winit.Modules.Syncing.BL.Classes
{
    public class EnhancedSqliteWriter
    {
        private readonly ILogger _logger = Log.ForContext<EnhancedSqliteWriter>();

        public async Task<int> WriteDataWithSchemaValidation(string sqliteFilePath, string tableName, List<object> data, int batchSize = 1000)
        {
            if (data == null || !data.Any())
            {
                return 0;
            }

            int totalWritten = 0;
            
            try
            {
                using var connection = new SqliteConnection($"Data Source={sqliteFilePath}");
                await connection.OpenAsync();

                // Get table columns from SQLite
                var tableColumns = await GetTableColumns(connection, tableName);
                if (!tableColumns.Any())
                {
                    _logger.Error("Table {TableName} not found in SQLite database", tableName);
                    return 0;
                }

                // Check if we're dealing with dynamic objects
                var firstItem = data.First();
                if (IsDynamicObject(firstItem))
                {
                    // Handle dynamic objects differently
                    return await WriteDynamicDataWithSchemaValidation(connection, tableName, data, tableColumns, batchSize);
                }

                // Get properties from the data objects
                var dataType = data.First().GetType();
                var properties = dataType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead)
                    .ToList();

                // Map object properties to table columns (case-insensitive)
                var propertyColumnMap = new Dictionary<PropertyInfo, string>();
                foreach (var prop in properties)
                {
                    var matchingColumn = tableColumns.FirstOrDefault(c => 
                        c.Equals(prop.Name, StringComparison.OrdinalIgnoreCase));
                    
                    if (matchingColumn != null)
                    {
                        propertyColumnMap[prop] = matchingColumn;
                    }
                }

                if (!propertyColumnMap.Any())
                {
                    _logger.Error("No matching columns found between object properties and table {TableName}", tableName);
                    return 0;
                }

                // Process data in batches
                for (int i = 0; i < data.Count; i += batchSize)
                {
                    var batch = data.Skip(i).Take(batchSize).ToList();
                    var written = await WriteBatch(connection, tableName, batch, propertyColumnMap);
                    totalWritten += written;
                }

                _logger.Information("Successfully wrote {RecordCount} records to table {TableName}", totalWritten, tableName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error writing data to table {TableName}", tableName);
                throw;
            }

            return totalWritten;
        }

        private bool IsDynamicObject(object obj)
        {
            // Check if the object is a dynamic type (ExpandoObject, DapperRow, etc.)
            return obj is System.Dynamic.IDynamicMetaObjectProvider || 
                   obj.GetType().Name.Contains("DynamicClass") ||
                   obj.GetType().Name.Contains("DapperRow");
        }

        private async Task<int> WriteDynamicDataWithSchemaValidation(SqliteConnection connection, string tableName, List<object> data, List<string> tableColumns, int batchSize)
        {
            int totalWritten = 0;

            // Process data in batches
            for (int i = 0; i < data.Count; i += batchSize)
            {
                var batch = data.Skip(i).Take(batchSize).ToList();
                var written = await WriteDynamicBatch(connection, tableName, batch, tableColumns);
                totalWritten += written;
            }

            _logger.Information("Successfully wrote {RecordCount} dynamic records to table {TableName}", totalWritten, tableName);
            return totalWritten;
        }

        private async Task<int> WriteDynamicBatch(SqliteConnection connection, string tableName, List<object> batch, List<string> tableColumns)
        {
            if (!batch.Any())
                return 0;

            // Build INSERT statement using all available columns
            var columnList = string.Join(", ", tableColumns);
            var parameterList = string.Join(", ", tableColumns.Select(c => "@" + c));
            
            var sql = $"INSERT OR REPLACE INTO {tableName} ({columnList}) VALUES ({parameterList})";

            int count = 0;
            using var transaction = connection.BeginTransaction();
            
            try
            {
                foreach (var item in batch)
                {
                    var parameters = new DynamicParameters();
                    
                    // Handle dynamic object properties
                    var dynamicDict = GetDynamicProperties(item);
                    
                    foreach (var column in tableColumns)
                    {
                        object value = null;
                        
                        // Try to get value from dynamic object (case-insensitive)
                        if (dynamicDict.ContainsKey(column))
                        {
                            value = dynamicDict[column];
                        }
                        else
                        {
                            // Try case-insensitive match
                            var matchingKey = dynamicDict.Keys.FirstOrDefault(k => 
                                k.Equals(column, StringComparison.OrdinalIgnoreCase));
                            if (matchingKey != null)
                            {
                                value = dynamicDict[matchingKey];
                            }
                        }
                        
                        // Handle special cases
                        if (value != null)
                        {
                            // Convert bool to int for SQLite
                            if (value is bool boolValue)
                            {
                                value = boolValue ? 1 : 0;
                            }
                            // Handle DateTime
                            else if (value is DateTime dateTime)
                            {
                                value = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            // Handle byte arrays
                            else if (value is byte[] bytes)
                            {
                                value = bytes;
                            }
                        }
                        else
                        {
                            // Convert null to DBNull.Value for Dapper compatibility
                            value = DBNull.Value;
                        }
                        
                        parameters.Add("@" + column, value);
                    }

                    await connection.ExecuteAsync(sql, parameters, transaction);
                    count++;
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.Error(ex, "Error writing dynamic batch to table {TableName}", tableName);
                throw;
            }

            return count;
        }

        private Dictionary<string, object> GetDynamicProperties(object dynamicObj)
        {
            var result = new Dictionary<string, object>();
            
            try
            {
                // Try to access as IDictionary first (most common for Dapper dynamic objects)
                var dict = dynamicObj as IDictionary<string, object>;
                if (dict != null)
                {
                    foreach (var kvp in dict)
                    {
                        result[kvp.Key] = kvp.Value ?? DBNull.Value;
                    }
                    return result;
                }

                // Try DapperRow: use reflection to get field names and values
                var type = dynamicObj.GetType();
                var fieldNamesProp = type.GetProperty("FieldNames", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                var valuesProp = type.GetProperty("values", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                if (fieldNamesProp != null && valuesProp != null)
                {
                    var fieldNames = fieldNamesProp.GetValue(dynamicObj) as string[];
                    var values = valuesProp.GetValue(dynamicObj) as object[];
                    if (fieldNames != null && values != null)
                    {
                        for (int i = 0; i < fieldNames.Length && i < values.Length; i++)
                        {
                            result[fieldNames[i]] = values[i] ?? DBNull.Value;
                        }
                        return result;
                    }
                }

                // Fallback: try to use reflection to get properties
                var properties = type.GetProperties();
                if (properties.Length > 0)
                {
                    foreach (var prop in properties)
                    {
                        try
                        {
                            var value = prop.GetValue(dynamicObj);
                            result[prop.Name] = value ?? DBNull.Value;
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Could not extract properties from dynamic object");
            }
            
            return result;
        }

        private async Task<List<string>> GetTableColumns(SqliteConnection connection, string tableName)
        {
            var sql = $"PRAGMA table_info({tableName})";
            var columns = await connection.QueryAsync(sql);
            return columns.Select(c => (string)c.name).ToList();
        }

        private async Task<int> WriteBatch(SqliteConnection connection, string tableName, List<object> batch, Dictionary<PropertyInfo, string> propertyColumnMap)
        {
            if (!batch.Any())
                return 0;

            // Build INSERT statement
            var columns = propertyColumnMap.Values.ToList();
            var columnList = string.Join(", ", columns);
            var parameterList = string.Join(", ", columns.Select(c => "@" + c));
            
            var sql = $"INSERT OR REPLACE INTO {tableName} ({columnList}) VALUES ({parameterList})";

            int count = 0;
            using var transaction = connection.BeginTransaction();
            
            try
            {
                foreach (var item in batch)
                {
                    var parameters = new DynamicParameters();
                    
                    foreach (var kvp in propertyColumnMap)
                    {
                        var value = kvp.Key.GetValue(item);
                        
                        // Handle special cases
                        if (value != null)
                        {
                            // Convert bool to int for SQLite
                            if (value is bool boolValue)
                            {
                                value = boolValue ? 1 : 0;
                            }
                            // Handle DateTime
                            else if (value is DateTime dateTime)
                            {
                                value = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            // Handle byte arrays
                            else if (value is byte[] bytes)
                            {
                                value = bytes;
                            }
                        }
                        
                        parameters.Add("@" + kvp.Value, value);
                    }

                    await connection.ExecuteAsync(sql, parameters, transaction);
                    count++;
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.Error(ex, "Error writing batch to table {TableName}", tableName);
                throw;
            }

            return count;
        }

        public async Task<int> ExecuteInsertQuery(string sqliteFilePath, string insertQuery, List<object> data, int batchSize = 1000)
        {
            if (string.IsNullOrEmpty(insertQuery) || data == null || !data.Any())
            {
                return 0;
            }

            int totalWritten = 0;

            try
            {
                using var connection = new SqliteConnection($"Data Source={sqliteFilePath}");
                await connection.OpenAsync();

                // Extract table name from INSERT query
                var tableName = ExtractTableName(insertQuery);
                if (string.IsNullOrEmpty(tableName))
                {
                    _logger.Warning("Could not extract table name from query, falling back to direct execution");
                    return await ExecuteDirectInsert(connection, insertQuery, data, batchSize);
                }

                // Try schema-aware write first
                try
                {
                    totalWritten = await WriteDataWithSchemaValidation(sqliteFilePath, tableName, data, batchSize);
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Schema-aware write failed for table {TableName}, falling back to direct execution", tableName);
                    totalWritten = await ExecuteDirectInsert(connection, insertQuery, data, batchSize);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error executing insert query");
                throw;
            }

            return totalWritten;
        }

        private string ExtractTableName(string insertQuery)
        {
            // Simple regex to extract table name from INSERT INTO table_name
            var match = System.Text.RegularExpressions.Regex.Match(
                insertQuery, 
                @"INSERT\s+(?:OR\s+\w+\s+)?INTO\s+(\w+)", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            return match.Success ? match.Groups[1].Value : null;
        }

        private async Task<int> ExecuteDirectInsert(SqliteConnection connection, string insertQuery, List<object> data, int batchSize)
        {
            int totalWritten = 0;
            
            // This is the fallback method - it just executes the provided query
            // This maintains backward compatibility but may fail if schema doesn't match
            
            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var item in data)
                {
                    await connection.ExecuteAsync(insertQuery, item, transaction);
                    totalWritten++;
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return totalWritten;
        }
    }
}