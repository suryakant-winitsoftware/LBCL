using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Npgsql;
using System.Data;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Transactions;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.Base.DL.DBManager;

public class SqliteDBManager : DBManagerBase
{
    private SqliteConnection? _dbConnection;
    public SqliteDBManager(IServiceProvider serviceProvider) : base(serviceProvider, ConnectionStringName.SQLite)
    {
        _dbConnection = new SqliteConnection(_connectionString);
    }

    public async Task<int> ExecuteNonQueryAsync(string sql, object? parameters = null,
        IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {
            if (connection == null)
            {
                using (connection = new SqliteConnection(_connectionString))
                {
                    return await connection!.ExecuteAsync(sql, parameters);
                }
            }
            else
            {
                ValidateTransaction(connection, transaction);
                return await connection!.ExecuteAsync(sql, parameters, transaction: transaction);
            }
        }
        catch (Exception ex)
        {
            // Log for debugging purposes - transaction rollback is handled by the owner
            Console.WriteLine($"ExecuteNonQueryAsync failed: {ex.Message}");
            Console.WriteLine($"SQL: {sql}");
            Console.WriteLine($"Parameters: {parameters}");
            
            // Don't rollback transaction here - let the transaction owner handle it
            throw;
        }
    }
    public void CloseConnection(SqliteConnection connection)
    {
        if (connection != null)
        {
            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
            connection.Dispose();
        }
    }
    public SqliteConnection GetConnection()
    {
        if (_dbConnection == null)
        {
            _dbConnection = new SqliteConnection(_connectionString);
        }
        return _dbConnection;
    }

    public void CloseConnection()
    {
        if (_dbConnection != null)
        {
            if (_dbConnection.State != ConnectionState.Closed)
            {
                _dbConnection.Close();
            }
            _dbConnection.Dispose();
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            _dbConnection = null;
        }
        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
    // Cross-platform method to ensure database file is released
    public async Task EnsureDatabaseReleasedAsync(string dbPath)
    {
        // Close the main connection
        CloseConnection();

        // Additional platform-specific steps
        try
        {
            // This works for both Android and Windows
            string forcedCloseConnString = $"Data Source={dbPath};Mode=ReadWrite;Pooling=False;Cache=Shared;";

            // Add journal mode for Android which can help with locking issues
            forcedCloseConnString += "Journal Mode=Delete;";

            using (var conn = new SqliteConnection(forcedCloseConnString))
            {
                try
                {
                    // Try to open and immediately close to release any locks
                    conn.Open();

                    // Execute PRAGMA to help release locks
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "PRAGMA locking_mode = NORMAL; PRAGMA journal_mode = DELETE;";
                        cmd.ExecuteNonQuery();
                    }

                    conn.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during connection test: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during platform-specific release: {ex.Message}");
        }

        // Clear all connection pools again
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        // Additional cleanup
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // Wait a moment to ensure everything is released
        await Task.Delay(1000);
    }
    //public async Task<int> ExecuteNonQueryAsync(string sql, SqliteConnection connection, SqliteTransaction transaction,
    //    IDictionary<string, object?>? parameters = null)
    //{
    //    return await connection.ExecuteAsync(sql, parameters, transaction: transaction);
    //}
    public async Task<T?> ExecuteScalarAsync<T>(string sql, object? parameters = null)
    {
        using SqliteConnection connection = new(_connectionString);
        return await connection.ExecuteScalarAsync<T>(sql, parameters);
    }

    public async Task<List<T>> ExecuteQueryAsync<T>(string sql, object? parameters = null,
        Type? type = null, IDbConnection? connection = null)
    {
        try
        {
            if (connection == null)
            {
                using (connection = new SqliteConnection(_connectionString))
                {
                    DefaultTypeMap.MatchNamesWithUnderscores = true;
                    IEnumerable<object> result = await connection.QueryAsync(!typeof(T).Namespace.StartsWith("System") ?
                    _serviceProvider.GetRequiredService<T>()?.GetType() : typeof(T), sql, parameters);
                    return result.ToList().OfType<T>().ToList();
                }
            }
            else
            {
                ValidateTransaction(connection, null);
                DefaultTypeMap.MatchNamesWithUnderscores = true;
                IEnumerable<object> result = await connection.QueryAsync(!typeof(T).Namespace.StartsWith("System") ?
                    _serviceProvider.GetRequiredService<T>()?.GetType() : typeof(T), sql, parameters);
                return result.ToList().OfType<T>().ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<DataTable> ExecuteQueryDataTableAsync(string sql, IDictionary<string, object?>? parameters = null)
    {
        using SqliteConnection conn = new(_connectionString);
        using SqliteCommand cmd = new(sql, conn);
        if (parameters != null)
        {
            foreach (KeyValuePair<string, object?> parameter in parameters)
            {
                _ = cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
            }
        }

        await conn.OpenAsync();
        using SqliteDataReader reader = await cmd.ExecuteReaderAsync();
        DataTable dataTable = new();
        dataTable.Load(reader);
        return dataTable;
    }

    public async Task<DataSet> ExecuteQueryDataSetAsync(string[] sqlQueries, IDictionary<string, object?>[]? parameters = null)
    {
        using SqliteConnection conn = new(_connectionString);
        await conn.OpenAsync();

        DataSet dataSet = new();

        for (int i = 0; i < sqlQueries.Length; i++)
        {
            using SqliteCommand cmd = new(sqlQueries[i], conn);
            if (parameters != null && parameters.Length > i)
            {
                foreach (KeyValuePair<string, object?> parameter in parameters[i])
                {
                    _ = cmd.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                }
            }

            using SqliteDataReader reader = await cmd.ExecuteReaderAsync();
            DataTable dataTable = new();
            dataTable.Load(reader);
            dataSet.Tables.Add(dataTable);
        }

        return dataSet;
    }


    /*
    public async Task<T> ExecuteSingleAsync<T>(string sql, IDictionary<string, object?>? parameters = null, Type? type = null)
    {
        using (var conn = new SqliteConnection(_connectionString))
        using (var cmd = new SqliteCommand(sql, conn))
        {
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }
            }

            await conn.OpenAsync();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    //var result = CreateInstance<T>();// Activator.CreateInstance<T>();
                    object? result = null;
                    if (type != null)
                    {
                        result = Activator.CreateInstance(type);
                    }
                    else
                    {
                        //result = Activator.CreateInstance<T>();
                        result = _serviceProvider.CreateInstance<T>();
                    }
                    foreach (var property in result.GetType().GetProperties())
                    {
                        try
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal(property.Name)))
                            {
                                //var value = reader.GetValue(reader.GetOrdinal(property.Name));
                                //property.SetValue(result, value);

                                object value;
                                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                                if (targetType == typeof(int) || targetType == typeof(Int32))
                                {
                                    value = reader.GetInt32(reader.GetOrdinal(property.Name));
                                }
                                else if (targetType == typeof(Int64) || targetType == typeof(Int64?))
                                {
                                    value = reader.GetInt64(reader.GetOrdinal(property.Name));
                                }
                                else if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                                {
                                    value = reader.GetDecimal(reader.GetOrdinal(property.Name));
                                }
                                else if (targetType == typeof(bool) || targetType == typeof(bool?))
                                {
                                    value = reader.GetBoolean(reader.GetOrdinal(property.Name));
                                }
                                else if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                                {
                                    value = reader.GetDateTime(reader.GetOrdinal(property.Name));
                                }
                                else
                                {
                                    value = reader.GetValue(reader.GetOrdinal(property.Name));
                                }


                                property.SetValue(result, value);

                                
                            }
                        }
                        catch (IndexOutOfRangeException)
                        {
                            // Handle the case when the column does not exist

                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            // Handle the case when the column does not exist

                        }
                        catch (Exception)
                        {
                            // Handle any other exceptions that occur while setting property values
                            throw;
                        }
                    }

                    return (T)result;
                }
            }

            return default;
        }
    }
    */
    public async Task<T> ExecuteSingleAsync<T>(string sql, object? parameters = null, Type? type = null,
        IDbConnection? connection = null, IDbTransaction? transaction = null
        )
    {
        try
        {
            if (connection == null)
            {
                using (connection = new SqliteConnection(_connectionString))
                {
                    return (T)await connection.QueryFirstOrDefaultAsync(!typeof(T).Namespace.StartsWith("System") ?
                    _serviceProvider.GetRequiredService<T>()?.GetType() : typeof(T), sql, parameters);
                }
            }
            else
            {
                ValidateTransaction(connection, transaction);
                return (T)await connection.QueryFirstOrDefaultAsync(!typeof(T).Namespace.StartsWith("System") ?
                _serviceProvider.GetRequiredService<T>()?.GetType() : typeof(T), sql, parameters, transaction);
            }
        }
        catch (Exception ex)
        {
            // Log for debugging purposes - transaction rollback is handled by the owner
            Console.WriteLine($"ExecuteSingleAsync failed: {ex.Message}");
            Console.WriteLine($"SQL: {sql}");
            Console.WriteLine($"Parameters: {parameters}");
            Console.WriteLine($"Type: {typeof(T).Name}");
            
            // Don't rollback transaction here - let the transaction owner handle it
            throw;
        }
    }

    //private T CreateInstance<T>(IDataReader reader)
    //{
    //    var result = Activator.CreateInstance<T>();
    //    var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

    //    foreach (var property in properties)
    //    {
    //        try
    //        {
    //            var columnName = property.Name;
    //            if (reader.HasColumn(columnName) && !reader.IsDBNull(reader.GetOrdinal(columnName)))
    //            {
    //                var value = reader.GetValue(reader.GetOrdinal(columnName));
    //                property.SetValue(result, value);
    //            }
    //        }
    //        catch (IndexOutOfRangeException)
    //        {
    //            // Handle the case when the column does not exist
    //        }
    //        catch (Exception)
    //        {
    //            // Handle any other exceptions that occur while setting property values
    //            throw;
    //        }
    //    }

    //    return result;
    //}
    public void AppendFilterCriteria(List<FilterCriteria> filterCriterias, StringBuilder sql, Dictionary<string, object?> parameters)
    {
        for (int i = 0; i < filterCriterias.Count; i++)
        {
            string paramName = $"filterParam{i}";
            FilterCriteria filterCriteria = filterCriterias[i];

            _ = sql.Append($"{filterCriteria.Name} ");

            switch (filterCriteria.Type)
            {
                case FilterType.NotEqual:
                    _ = sql.Append($"!= @{paramName}");
                    break;
                case FilterType.Equal:
                    _ = sql.Append($"= @{paramName}");
                    break;
                case FilterType.Like:
                    _ = sql.Append($"LIKE '%' + @{paramName} + '%'");
                    break;
                case FilterType.Is:
                    _ = sql.Append($"\"{filterCriteria.Name}\" IS NULL");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (i < filterCriterias.Count - 1)
            {
                _ = sql.Append(" AND ");
            }

            parameters.Add(paramName, filterCriteria.Value);
        }
    }

    public void AppendSortCriteria(List<SortCriteria> sortCriterias, StringBuilder sql)
    {
        for (int i = 0; i < sortCriterias.Count; i++)
        {
            SortCriteria sortCriteria = sortCriterias[i];

            _ = sql.Append($"[{sortCriteria.SortParameter}] {(sortCriteria.Direction == SortDirection.Asc ? "ASC" : "DESC")}");

            if (i < sortCriterias.Count - 1)
            {
                _ = sql.Append(", ");
            }
        }
    }



    public T ConvertDataTableToObject<T>(DataRow row, IFactory? factory = null, Dictionary<string, string?>? columnMappings = null)
    {
        T? result = factory != null ? (T)factory.CreateInstance() : Activator.CreateInstance<T>();
        foreach (DataColumn column in row.Table.Columns)
        {
            string? propertyName = columnMappings != null && columnMappings.TryGetValue(column.ColumnName, out string aliasPropertyName)
                ? aliasPropertyName
                : column.ColumnName;
            PropertyInfo property = result.GetType().GetProperty(propertyName);
            if (property != null && row[column] != DBNull.Value)
            {
                if (property.PropertyType == typeof(DateTime))
                {
                    // Handle DateTime conversion to string
                    if (DateTime.TryParse(row[column].ToString(), out DateTime dateTimeValue))
                    {
                        property.SetValue(result, dateTimeValue);
                    }
                }
                else if (property.PropertyType == typeof(DateTime?))
                {
                    // Handle nullable DateTime conversion to string
                    if (DateTime.TryParse(row[column].ToString(), out DateTime dateTimeValue))
                    {
                        property.SetValue(result, (DateTime?)dateTimeValue);
                    }
                }
                else if (property.PropertyType == typeof(decimal))
                {
                    if (decimal.TryParse(row[column].ToString(), out decimal decimalValue))
                    {
                        property.SetValue(result, decimalValue);
                    }
                }
                else if (property.PropertyType == typeof(int))
                {
                    if (row[column] is long longValue)
                    {
                        // Explicitly convert the Int64 to Int32
                        property.SetValue(result, (int)longValue);
                    }
                }
                else if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
                {
                    // Handle boolean conversions
                    if (row[column] is int intValue)
                    {
                        property.SetValue(result, intValue != 0);
                    }
                    else if (row[column] is long longValueBool)
                    {
                        property.SetValue(result, longValueBool != 0);
                    }
                    else if (row[column] is bool boolValue)
                    {
                        property.SetValue(result, boolValue);
                    }
                }
                else
                {
                    // For other property types, set the value directly
                    property.SetValue(result, row[column]);
                }
            }
        }

        return result;
    }

    public async Task<List<string>?> CheckIfUIDExistsInDB(string tableName, List<string> uIDs,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {
            Dictionary<string, object?> parameters = new()
            {
                {"@UIDs",  uIDs}
            };
            string sql = $@"SELECT uid AS UID FROM {tableName} WHERE uid IN @UIDs";
            return await ExecuteQueryAsync<string>(sql, parameters, null, connection);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    public async Task<string?> CheckIfUIDExistsInDB(string tableName, string uID,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {
            Dictionary<string, object?> parameters = new()
            {
                {"@UID",  uID}
            };
            string sql = $@"SELECT uid AS UID FROM {tableName} WHERE uid = @UID";
            return await ExecuteSingleAsync<string>(sql, parameters, null, connection, transaction);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    public async Task<int> DeleteByUID(string tableName, List<string> uIDs,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {
            Dictionary<string, object?> parameters = new()
            {
                {"@UIDs",  uIDs}
            };
            string sql = $@"DELETE FROM {tableName} WHERE uid IN @UIDs";
            return await ExecuteNonQueryAsync(sql, parameters, connection, transaction);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    public async Task<int> DeleteByUID(string tableName, string uID,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {
            Dictionary<string, object?> parameters = new()
            {
                {"@UID",  uID}
            };
            string sql = $@"DELETE FROM {tableName} WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters, connection, transaction);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

}

// Extension method to check if the reader has a specific column
public static class DataReaderExtensions
{
    public static bool HasColumn(this IDataReader reader, string columnName)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}



public class DecimalTypeHandler : SqlMapper.TypeHandler<decimal>
{
    public override decimal Parse(object value)
    {
        if (value is string strValue)
        {
            return decimal.Parse(strValue);
        }
        else if (value is double doubleValue)
        {
            return Convert.ToDecimal(doubleValue);
        }
        else
        {
            return value is long longValue ? Convert.ToDecimal(longValue) : throw new ArgumentOutOfRangeException(nameof(value));
        }
    }

    public override void SetValue(IDbDataParameter cmd, decimal value)
    {
        cmd.DbType = DbType.Decimal;
        cmd.Value = value;
    }
}

//public class InterfaceTypeHandler<T> : SqlMapper.TypeHandler<T> where T : class
//{
//    private readonly IServiceProvider _serviceProvider;
//    private readonly JsonSerializerSettings _jsonSettings;

//    public InterfaceTypeHandler(IServiceProvider serviceProvider, JsonSerializerSettings jsonSerializerSettings)
//    {
//        _serviceProvider = serviceProvider;
//        _jsonSettings = jsonSerializerSettings;
//    }

//    public override T Parse(object value)
//    {
//        if (value == null)
//            return null;

//        if (value is string json)
//        {
//            // If the value is a JSON string, deserialize it
//            var concreteType = GetConcreteType(typeof(T));
//            return (T)JsonConvert.DeserializeObject(json, concreteType, _jsonSettings);
//        }

//        // If it's not a string, it might be already the correct type
//        if (value is T typedValue)
//            return typedValue;

//        // If all else fails, try to convert it
//        return (T)Convert.ChangeType(value, typeof(T));
//    }

//    public override void SetValue(IDbDataParameter parameter, T value)
//    {
//        if (value == null)
//        {
//            parameter.Value = DBNull.Value;
//        }
//        else
//        {
//            parameter.DbType = DbType.String;
//            parameter.Value = JsonConvert.SerializeObject(value, _jsonSettings);
//        }
//    }

//    private Type GetConcreteType(Type interfaceType)
//    {
//        if (!interfaceType.IsInterface)
//            return interfaceType;

//        var implementation = _serviceProvider.GetService(interfaceType);
//        if (implementation != null)
//            return implementation.GetType();

//        throw new InvalidOperationException($"No implementation found for interface {interfaceType.FullName}");
//    }
//}