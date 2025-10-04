using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Text;
using System.Transactions;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Base.DL.DBManager;

public class PostgresDBManager : DBManagerBase
{
    public PostgresDBManager(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config, ConnectionStringName.PostgreSQL)
    {
    }

    /// <summary>
    /// Creates a new PostgreSQL connection
    /// </summary>
    /// <returns>NpgsqlConnection</returns>
    protected new NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
    /// <summary>
    /// This method is used to execute SQL statements that do not return any rows, such as INSERT, UPDATE, and DELETE statements.
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public async Task<int> ExecuteNonQueryAsync(string sql, object? parameters = null)
    {
        using NpgsqlConnection connection = new(_connectionString);
        return await connection.ExecuteAsync(sql, parameters);
    }
    //Ramana
    public async Task<int> ExecuteStoredProcedureAsync(string storedProcedureName, IDictionary<string, object?>?
        parameters = null)
    {
        try
        {
            using NpgsqlConnection conn = new(_connectionString);
            using NpgsqlCommand cmd = new(storedProcedureName, conn);
            cmd.CommandType = CommandType.Text;//.StoredProcedure; // Set command type to stored procedure

            if (parameters != null)
            {
                // Create DataTable
                DataTable dataTable = new();
                _ = dataTable.Columns.Add("sales_order_uid", typeof(string));
                _ = dataTable.Columns.Add("route_uid", typeof(string));
                _ = dataTable.Columns.Add("delivery_date", typeof(DateTime));
                _ = dataTable.Columns.Add("action_type", typeof(string));

                foreach (KeyValuePair<string, object> parameter in parameters)
                {
                    if (parameter.Key == "in_udt_order_assignment")
                    {
                        List<Tuple<string, string, DateTime, string>> vkList = (List<Tuple<string, string, DateTime, string>>)parameter.Value;

                        foreach (Tuple<string, string, DateTime, string> vk in vkList)
                        {
                            _ = dataTable.Rows.Add(vk.Item1, vk.Item2, vk.Item3, vk.Item4);
                        }

                        NpgsqlParameter inputParameter = new("in_udt_order_assignment", NpgsqlTypes.NpgsqlDbType.Array)
                        {
                            Value = dataTable
                        };
                        _ = cmd.Parameters.Add(inputParameter);
                    }
                    else
                    {
                        _ = cmd.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                    }
                }
            }

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            // Log the exception for debugging purposes
            Console.WriteLine($"Error in ExecuteStoredProcedureAsync: {ex.Message}");
            throw; // Rethrow the exception for upper-level handling
        }
    }

    public async Task<int> ExecuteNonQueryAsync(string sql, IDbConnection? connection = null, IDbTransaction? transaction = null,
        object? parameters = null)
    {
        try
        {
            if (connection == null)
            {
                using (connection = new NpgsqlConnection(_connectionString))
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
        catch (Exception)
        {
            RollbackTransaction(transaction);
            throw;
        }
    }

    /// <summary>
    /// This method is used to execute SQL statements that return a single value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public async Task<T?> ExecuteScalarAsync<T>(string sql, object? parameters = null, IDbConnection? connection = null)
    {
        if (connection == null)
            connection = new NpgsqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<T>(sql, parameters);
    }
    /// <summary>
    /// Executes a SQL query and returns a list of results as objects of type T
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public async Task<List<T>> ExecuteQueryAsync<T>(string sql, object? parameters = null, Type? type = null,
        IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {
            if (connection == null)
            {
                using (connection = new NpgsqlConnection(_connectionString))
                {
                    DefaultTypeMap.MatchNamesWithUnderscores = true;
                    
                    // Special handling for dynamic type
                    if (typeof(T) == typeof(object) || typeof(T).Name.Contains("DynamicClass"))
                    {
                        var dynamicResult = await connection.QueryAsync(sql, parameters);
                        return dynamicResult.Cast<T>().ToList();
                    }
                    
                    IEnumerable<object> result = await connection.QueryAsync(!typeof(T).Namespace.StartsWith("System") ?
                        _serviceProvider.GetRequiredService<T>()?.GetType() : typeof(T), sql, parameters);
                    return result.ToList().OfType<T>().ToList();
                }
            }
            else
            {
                ValidateTransaction(connection, transaction);
                
                // Special handling for dynamic type
                if (typeof(T) == typeof(object) || typeof(T).Name.Contains("DynamicClass"))
                {
                    var dynamicResult = await connection.QueryAsync(sql, parameters);
                    return dynamicResult.Cast<T>().ToList();
                }
                
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

    /// <summary>
    /// Executes a SQL query and returns a DataTab0le with the results
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public async Task<DataTable> ExecuteQueryDataTableAsync(string sql, IDictionary<string, object?>? parameters = null)
    {
        using NpgsqlConnection conn = new(_connectionString);
        using NpgsqlCommand cmd = new(sql, conn);
        if (parameters != null)
        {
            foreach (KeyValuePair<string, object> parameter in parameters)
            {
                _ = cmd.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
            }
        }

        await conn.OpenAsync();
        using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        DataTable dataTable = new();
        dataTable.Load(reader);
        return dataTable;
    }

    public async Task<DataSet> ExecuteQueryDataSetAsync(string sql, IDictionary<string, object>? parameters = null)
    {
        using NpgsqlConnection conn = new(_connectionString);
        await conn.OpenAsync();

        using NpgsqlCommand cmd = new(sql, conn);
        if (parameters != null)
        {
            foreach (KeyValuePair<string, object> parameter in parameters)
            {
                _ = cmd.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
            }
        }
        DataSet dataSet = new();
        using (NpgsqlDataAdapter da = new(cmd))
        {
            _ = da.Fill(dataSet);
        }

        return dataSet;
    }

    /// <summary>
    /// Executes a SQL query and returns a single object of type T
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    /*
    public async Task<T?> ExecuteSingleAsync<T>(string sql, IDictionary<string, object?>? parameters = null, Type? type = null)
    {
        using NpgsqlConnection conn = new(_connectionString);
        using NpgsqlCommand cmd = new(sql, conn);
        if (parameters != null)
        {
            foreach (KeyValuePair<string, object?> parameter in parameters)
            {
                _ = cmd.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
            }
        }

        await conn.OpenAsync();
        using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            //var result = CreateInstance<T>();// Activator.CreateInstance<T>();
            object? result = type != null ? Activator.CreateInstance(type) : _serviceProvider.CreateInstance<T>();
            foreach (PropertyInfo property in result.GetType().GetProperties())
            {
                try
                {
                    if (!reader.IsDBNull(reader.GetOrdinal(property.Name)))
                    {
                        object value = reader.GetValue(reader.GetOrdinal(property.Name));
                        property.SetValue(result, value);
                    }
                }
                catch (IndexOutOfRangeException)
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

        return default;
    }
    */
    public async Task<T?> ExecuteSingleAsync<T>(string sql, object? parameters = null,
        Type? type = null, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {
            if (connection == null)
            {
                using (connection = new NpgsqlConnection(_connectionString))
                {
                    object? result = await connection.QueryFirstOrDefaultAsync(!typeof(T).Namespace!.StartsWith("System") ?
                            _serviceProvider.GetRequiredService<T>()!.GetType()! : typeof(T), sql, parameters);
                    return (T)result;
                }
            }
            else
            {
                ValidateTransaction(connection, transaction);
                object? result = await connection.QueryFirstOrDefaultAsync(!typeof(T).Namespace!.StartsWith("System") ?
                        _serviceProvider.GetRequiredService<T>()!.GetType()! : typeof(T), sql, parameters, transaction: transaction);
                return (T)result;
            }
        }
        catch (Exception)
        {
            RollbackTransaction(transaction);
            throw;
        }
    }
    public void CloseConnection(NpgsqlConnection connection)
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
    //srinadh added
    public void AppendFilterCriteria<T>(List<FilterCriteria> filterCriterias, StringBuilder sql,
        Dictionary<string, object?> parameters) where T : class
    {
        for (int i = 0; i < filterCriterias.Count; i++)
        {
            string paramName = $"filterParam{i}";
            FilterCriteria filterCriteria = filterCriterias[i];

            object? parsedValue = ParseValueToModelFieldDataType<T>(filterCriteria.Name.Replace("_", ""), CommonFunctions.GetStringValue(filterCriteria.Value));

            // Default to Equal if Type is not set or is invalid
            FilterType filterType = filterCriteria.Type != 0 ? filterCriteria.Type : FilterType.Equal;

            switch (filterType)
            {
                case FilterType.NotEqual:
                case FilterType.Equal:
                    AppendEqualNotEqualCondition(sql, paramName, filterCriteria.Name, parsedValue, filterCriteria.Value, parameters, filterType);
                    break;

                case FilterType.Like:
                case FilterType.Contains:
                    AppendLikeCondition(sql, paramName, filterCriteria.Name, parsedValue, filterCriteria.Value, parameters);
                    break;

                case FilterType.Is:
                    _ = sql.Append($"{filterCriteria.Name} IS NULL");
                    break;
                case FilterType.In:
                    Array inValues = ConvertToArray(filterCriteria.Value);
                    AppendInCondition(sql, paramName, filterCriteria.Name, inValues, filterCriteria.DataType, parameters);
                    break;
                case FilterType.Between:
                    Array Values = ConvertToArray(filterCriteria.Value);
                    if (Values is Array array && array.Length >= 2)
                    {
                        //var betweenValues = new object[2];
                        //Array.Copy(array, betweenValues, 2);
                        AppendBetweenCondition(sql, $"{paramName}_start", $"{paramName}_end", filterCriteria.Name, array.GetValue(0), array.GetValue(1), parameters);
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid values for BETWEEN filter type for column {filterCriteria.Name}");
                    }
                    break;
                case FilterType.GreaterThanOrEqual or FilterType.LessThanOrEqual:
                    AppendLessOrGreaterThanEqualCondition(sql, paramName, filterCriteria.Name, parsedValue, filterCriteria.Value, parameters, filterType);
                    break;
                default:
                    // Default case: treat as Equal
                    AppendEqualNotEqualCondition(sql, paramName, filterCriteria.Name, parsedValue, filterCriteria.Value, parameters, FilterType.Equal);
                    break;
            }

            if (i < filterCriterias.Count - 1)
            {
                _ = filterCriteria.FilterMode == FilterMode.And ? sql.Append(" AND ") : sql.Append(" OR ");
            }
        }
    }
    public object? ParseValueToModelFieldDataType<T>(string fieldName, string fieldValueAsString) where T : class
    {
        // Get the field information based on the field name
        //        var propertyInfo = typeof(T).GetInterfaces()
        //.SelectMany(interfaceType => interfaceType.GetProperties())
        //.FirstOrDefault(p => p.Name == fieldName);
        PropertyInfo? propertyInfo = _serviceProvider.GetRequiredService<T>().GetType().GetProperty
            (fieldName);
        if (propertyInfo != null)
        {
            // Get the data type of the field
            Type fieldType = propertyInfo.PropertyType;
            try
            {
                // Parse the string value to the appropriate data type
                if (fieldType == typeof(string))
                {
                    return fieldValueAsString;
                }
                else if (fieldType == typeof(DateTime?))
                {
                    return DateTime.Parse(fieldValueAsString);
                }
                object? parsedValue = Convert.ChangeType(fieldValueAsString, fieldType);
                // For reference types, explicitly cast to the property type
                if (!fieldType.IsValueType)
                {
                    parsedValue = fieldType.IsAssignableFrom(parsedValue.GetType()) ? parsedValue : null;
                }
                return parsedValue;
            }
            catch (Exception ex)
            {
                // Handle conversion errors (e.g., invalid format)
                Console.WriteLine($"Error converting value for field '{fieldName}': {ex.Message}");
                return null;
            }
        }
        return null;
    }
    private void AppendBetweenCondition(StringBuilder sql, string paramNameStart, string paramNameEnd,
        string fieldName, object startValue, object endValue, Dictionary<string, object> parameters)
    {
        _ = $"{paramNameStart}";
        _ = $"{paramNameEnd}";

        if (DateTime.TryParse(CommonFunctions.GetStringValue(startValue), out DateTime startDateTime) &&
            DateTime.TryParse(CommonFunctions.GetStringValue(endValue), out DateTime endDateTime))
        {
            if (startDateTime.TimeOfDay != TimeSpan.Zero || endDateTime.TimeOfDay != TimeSpan.Zero)
            {
                _ = sql.Append($"{fieldName} ");
                _ = sql.Append("BETWEEN :start").Append(paramNameStart).Append(" AND :end").Append(paramNameEnd);
                parameters.Add("start" + paramNameStart, startDateTime);
                parameters.Add("end" + paramNameEnd, endDateTime);
            }
            else
            {
                _ = sql.Append($"TO_CHAR({fieldName},'YYYY-MM-DD') ");
                _ = sql.Append("BETWEEN :start").Append(paramNameStart).Append(" AND :end").Append(paramNameEnd);
                parameters.Add("start" + paramNameStart, startDateTime.Date.ToString("yyyy-MM-dd"));
                parameters.Add("end" + paramNameEnd, endDateTime.Date.ToString("yyyy-MM-dd"));
            }
        }
        else if (TimeSpan.TryParse(CommonFunctions.GetStringValue(startValue), out TimeSpan startTimeSpan) &&
                 TimeSpan.TryParse(CommonFunctions.GetStringValue(endValue), out TimeSpan endTimeSpan))
        {
            _ = sql.Append(fieldName);
            _ = sql.Append("BETWEEN :start").Append(paramNameStart).Append(" AND :end").Append(paramNameEnd);
            parameters.Add("start" + paramNameStart, startTimeSpan.ToString());
            parameters.Add("end" + paramNameEnd, endTimeSpan.ToString());
        }
    }

    private Array ConvertToArray(object value)
    {
        if (value is IEnumerable enumerable and not string)
        {
            // If it's enumerable and not a string, convert to array
            return enumerable.Cast<object>().ToArray();
        }
        else if (value != null && value.GetType().IsArray)
        {
            // If it's an array, return it
            return (Array)value;
        }
        else
        {
            // Convert the single value to a single-element array
            return new object[] { value };
        }
    }

    private void AppendInCondition(StringBuilder sql, string paramName, string columnName, Array rawValue,
        Type dataType, Dictionary<string, object> parameters)
    {

        if (rawValue is Array inValues)
        {
            AppendInConditionForArray(sql, paramName, columnName, inValues, dataType, parameters);
        }
        else
        {
            throw new ArgumentException($"Invalid value for IN filter type for column {columnName}");
        }
    }

    private void AppendInConditionForArray(StringBuilder sql, string paramName, string columnName, Array rawValue,
        Type dataType, Dictionary<string, object> parameters)
    {
        // Use parameterized query to avoid SQL injection
        List<string> inParam = new();
        int j = 0;

        foreach (object? value in rawValue)
        {
            string paramKey = $"{paramName}_in{j++}";
            inParam.Add($"@{paramKey}");

            // Convert each element of the array to the specified data type
            object parsedValue = Convert.ChangeType(value, dataType);
            parameters.Add(paramKey, parsedValue);
        }

        // sql.Append($"\"{columnName}\" IN ({string.Join(", ", inParam)})");
        _ = sql.Append($" {columnName} IN ({string.Join(", ", inParam)})");
    }

    //private void AppendEqualNotEqualCondition(StringBuilder sql, string paramName, string fieldName, object parsedValue, object filterValue, Dictionary<string, object> parameters, FilterType filterCriteriaType, bool IsSnakeCase = false)
    //{
    //    if (IsSnakeCase) sql.Append(fieldName); else sql.Append($"\"{fieldName}\" ");

    //    if (parsedValue != null && parsedValue.GetType() != typeof(string))
    //    {
    //        sql.Append(filterCriteriaType == FilterType.NotEqual ? "!= :" : "= :");
    //        sql.Append(paramName);
    //        parameters.Add(paramName, parsedValue);
    //    }
    //    else
    //    {
    //        sql.Append(filterCriteriaType == FilterType.NotEqual ? "!= :" : "= :");
    //        sql.Append(paramName);
    //        parameters.Add(paramName, filterValue);
    //    }
    //}
    private void AppendEqualNotEqualCondition(StringBuilder sql, string paramName, string fieldName, object? parsedValue,
        object filterValue, Dictionary<string, object> parameters, FilterType filterCriteriaType)
    {
        if (parsedValue != null && parsedValue.GetType() == typeof(DateTime))
        {
            DateTime dateTimeValue = (DateTime)parsedValue;
            if (dateTimeValue.TimeOfDay != TimeSpan.Zero)
            {
                _ = sql.Append($"{fieldName} ");
                _ = sql.Append(filterCriteriaType == FilterType.NotEqual ? " != @" : " = @");
                _ = sql.Append(paramName);
                parameters.Add(paramName, dateTimeValue);
            }
            else
            {
                _ = sql.Append($"TO_CHAR({fieldName},'YYYY-MM-DD') ");
                _ = sql.Append(filterCriteriaType == FilterType.NotEqual ? " != @" : " = @");
                _ = sql.Append(paramName);
                parameters.Add(paramName, dateTimeValue.Date.ToString("yyyy-MM-dd"));
            }
        }
        else if (parsedValue != null && parsedValue.GetType() != typeof(string))
        {
            _ = sql.Append(fieldName);
            _ = sql.Append(filterCriteriaType == FilterType.NotEqual ? " != @" : " = @");
            _ = sql.Append(paramName);
            parameters.Add(paramName, parsedValue);
        }
        else
        {
            _ = sql.Append(fieldName);
            _ = sql.Append(filterCriteriaType == FilterType.NotEqual ? " != @" : " = @");
            _ = sql.Append(paramName);
            parameters.Add(paramName, filterValue);
        }
    }
    private void AppendLikeCondition(StringBuilder sql, string paramName, string fieldName, object? parsedValue,
        object filterValue, Dictionary<string, object> parameters)
    {
        if (parsedValue != null && parsedValue.GetType() == typeof(DateTime))
        {
            DateTime dateTimeValue = (DateTime)parsedValue;
            if (dateTimeValue.TimeOfDay != TimeSpan.Zero)
            {
                _ = sql.Append($"TO_CHAR({fieldName},'YYYY-MM-DD HH24:MI:SS.US') ");
                _ = sql.Append("ILIKE '%' || :");
                _ = sql.Append(paramName);
                _ = sql.Append(" || '%'");
                parameters.Add(paramName, dateTimeValue);
            }
            else
            {
                _ = sql.Append($"TO_CHAR({fieldName},'YYYY-MM-DD') ");
                _ = sql.Append("ILIKE '%' || :");
                _ = sql.Append(paramName);
                _ = sql.Append(" || '%'");
                parameters.Add(paramName, dateTimeValue.Date.ToString("yyyy-MM-dd"));
            }
        }
        else if (parsedValue != null && parsedValue.GetType() != typeof(string))
        {
            _ = sql.Append($"{fieldName} ");
            _ = sql.Append("ILIKE '%' || :");
            _ = sql.Append(paramName);
            _ = sql.Append(" || '%'");
            parameters.Add(paramName, parsedValue);
        }
        else
        {
            _ = sql.Append($"{fieldName} ");
            _ = sql.Append("ILIKE '%' || :");
            _ = sql.Append(paramName);
            _ = sql.Append(" || '%'");
            parameters.Add(paramName, filterValue);
        }
    }
    private void AppendLessOrGreaterThanEqualCondition(StringBuilder sql, string paramName, string fieldName, object? parsedValue, object filterValue, Dictionary<string, object> parameters, FilterType filterCriteriaType)
    {
        string operatorString = filterCriteriaType == FilterType.GreaterThanOrEqual ? " >= " : " <= ";

        if (parsedValue != null && parsedValue.GetType() == typeof(DateTime))
        {
            DateTime dateTimeValue = (DateTime)parsedValue;
            if (dateTimeValue.TimeOfDay != TimeSpan.Zero)
            {
                _ = sql.Append($"{fieldName} {operatorString} :{paramName}");
                parameters.Add(paramName, dateTimeValue);
            }
            else
            {
                _ = sql.Append($"TO_CHAR({fieldName},'YYYY-MM-DD') {operatorString} :{paramName}");
                parameters.Add(paramName, dateTimeValue.Date.ToString("yyyy-MM-dd"));
            }
        }
        else if (parsedValue != null && parsedValue.GetType() != typeof(string))
        {
            _ = sql.Append($"{fieldName} {operatorString} :{paramName}");
            parameters.Add(paramName, parsedValue);
        }
        else
        {
            sql.Append($"{fieldName} {operatorString} :{paramName}");
            parameters.Add(paramName, filterValue);
        }

    }
    public void AppendSortCriteria(List<SortCriteria> sortCriterias, StringBuilder sql, bool IsSnakeCase = true)
    {
        for (int i = 0; i < sortCriterias.Count; i++)
        {
            SortCriteria sortCriteria = sortCriterias[i];
            _ = sql.Append($"{sortCriteria.SortParameter} {(sortCriteria.Direction == SortDirection.Asc ? "ASC" : "DESC")}");
            if (i < sortCriterias.Count - 1)
            {
                _ = sql.Append(", ");
            }
        }
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
            string sql = $@"SELECT uid AS UID FROM {tableName} WHERE uid = ANY(@UIDs) ";
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
            string sql = $@"DELETE FROM {tableName} WHERE uid = ANY(@UIDs)";
            return await ExecuteNonQueryAsync(sql, connection, transaction, parameters);
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
            return await ExecuteNonQueryAsync(sql, connection, transaction, parameters);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    /// <summary>
    /// /
    /// </summary>
    /// <param name="Key"></param>
    /// <returns></returns>
    public async Task<string?> DoesTableExist(string tableName)
    {
        try
        {
            Dictionary<string, object?> parameters = new()
                {
                    {"@tableName", tableName}
                };
            string sql = $@"SELECT [name] FROM sys.tables  WHERE [name] = @tableName";
            return await ExecuteSingleAsync<string>(sql, parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Key"></param>
    /// <returns></returns>
    public async Task<string?> GetSettingValueByKey(string Key)
    {
        try
        {
            Dictionary<string, object?> parameters = new()
                {
                    {"@Key", Key}
                };
            string sql = $@"SELECT [value] FROM tblsetting  WHERE [key] = @Key";
            return await ExecuteSingleAsync<string>(sql, parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }
    protected async Task<T?> Query<T>(Func<IDbConnection, Task<T?>> func)
    {
        try
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                return await func.Invoke(connection);
            }
        }
        catch (Exception)
        {

            throw;
        }
    }
}
