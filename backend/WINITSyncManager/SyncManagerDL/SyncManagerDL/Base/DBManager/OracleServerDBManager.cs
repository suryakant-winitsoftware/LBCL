using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Nest;

namespace SyncManagerDL.Base.DBManager
{
    public class OracleServerDBManager : DBManagerBase
    {

        public OracleServerDBManager(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config, ConnectionStringName.OracleServer)
        {
        }

        /// <summary>
        /// Executes a SQL query asynchronously and returns the results as a list of objects of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the objects to return.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="parameters">The parameters for the SQL query (optional).</param>
        /// <param name="type">The type of the object to return (optional).</param>
        /// <param name="connection">The database connection to use (optional). If null, a new connection is created.</param>
        /// <returns>A list of objects of type <typeparamref name="T"/> representing the results of the query.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during query execution.</exception>
        public async Task<List<T>> ExecuteQueryAsync<T>(string sql, object? parameters = null, Type? type = null, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            try
            {
                if (connection == null)
                {
                    using (connection = new OracleConnection(_connectionString))
                    {
                        connection.Open();
                        DefaultTypeMap.MatchNamesWithUnderscores = true;
                        //var directResult = connection.QueryAsync<object>(sql, parameters).GetAwaiter().GetResult();
                        //IEnumerable<object> result1 = connection.QueryAsync<object>(sql, parameters).GetAwaiter().GetResult();
                        IEnumerable<object> result = connection.QueryAsync(!typeof(T).Namespace.StartsWith("System") ?
                            _serviceProvider.GetRequiredService<T>()?.GetType() : typeof(T), sql, parameters).GetAwaiter().GetResult();
                        return result.ToList().OfType<T>().ToList();
                    }
                }
                else
                {
                    ValidateTransaction(connection, transaction);
                    DefaultTypeMap.MatchNamesWithUnderscores = true;
                    IEnumerable<object> result =   connection.QueryAsync(!typeof(T).Namespace.StartsWith("System") ?
                        _serviceProvider.GetRequiredService<T>()?.GetType() : typeof(T), sql, parameters, transaction: transaction).GetAwaiter().GetResult();
                    return result.ToList().OfType<T>().ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Executes a query and returns the first result as an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to return.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="parameters">The parameters for the SQL query.</param>
        /// <param name="type">The type of the object to return (optional).</param>
        /// <param name="connection">The database connection to use (optional).</param>
        /// <param name="transaction">The transaction to use (optional).</param>
        /// <returns>The first result of the query, or null if no result is found.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during query execution.</exception>
        public async Task<T?> ExecuteSingleAsync<T>(string sql, object? parameters = null,
        Type? type = null, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            try
            {
                if (connection == null)
                {
                    using (connection = new OracleConnection(_connectionString))
                    {

                        connection.Open();
                        object? result =   connection.QueryFirstOrDefaultAsync(!typeof(T).Namespace!.StartsWith("System") ?
                                _serviceProvider.GetRequiredService<T>()!.GetType()! : typeof(T), sql, parameters).GetAwaiter().GetResult();
                        return (T)result;
                    }
                }
                else
                {
                    ValidateTransaction(connection, transaction);
                    object? result =   connection.QueryFirstOrDefaultAsync(!typeof(T).Namespace!.StartsWith("System") ?
                            _serviceProvider.GetRequiredService<T>()!.GetType()! : typeof(T), sql, parameters, transaction: transaction).GetAwaiter().GetResult();
                    return (T)result;
                }
            }
            catch (Exception)
            {
                RollbackTransaction(transaction);
                throw;
            }
        }
        /// <summary>
        /// This method is used to execute SQL statements that do not return any rows, such as INSERT, UPDATE, and DELETE statements.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<int> ExecuteNonQueryAsync(string sql, object? parameters = null)
        {
            using OracleConnection connection = new(_connectionString);
            return   connection.ExecuteAsync(sql, parameters).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Executes a SQL non-query command asynchronously and returns the number of affected rows.
        /// </summary>
        /// <param name="sql">The SQL command to execute.</param>
        /// <param name="connection">Optional: The database connection to use. If not provided, a new connection is created.</param>
        /// <param name="transaction">Optional: The transaction to use with the command, if applicable.</param>
        /// <param name="parameters">Optional: The parameters for the SQL command.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the number of rows affected by the command.</returns>
        /// <exception cref="ArgumentException">Thrown when the provided transaction is not associated with the provided connection.</exception>
        /// <exception cref="Exception">Thrown when an error occurs during command execution.</exception>
        public async Task<int> ExecuteNonQueryAsync(string sql, IDbConnection? connection = null, IDbTransaction? transaction = null,
        object? parameters = null)
        {
            try
            {
                if (connection == null)
                {
                    using (connection = new OracleConnection(_connectionString))
                    {
                        connection.Open();
                        return   connection!.ExecuteAsync(sql, parameters).GetAwaiter().GetResult();
                    }
                }
                else
                {
                    ValidateTransaction(connection, transaction);
                    return   connection!.ExecuteAsync(sql, parameters, transaction: transaction).GetAwaiter().GetResult();
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
        public async Task<T?> ExecuteScalarAsync<T>(string sql, object? parameters = null)
        {
            using OracleConnection connection = new(_connectionString);
            return   connection.ExecuteScalarAsync<T>(sql, parameters).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Checks if the specified UIDs exist in the specified table in the database.
        /// </summary>
        /// <param name="tableName">The name of the table to check.</param>
        /// <param name="uIDs">The list of UIDs to check for existence.</param>
        /// <param name="connection">The database connection to use (optional). If null, a new connection is created.</param>
        /// <param name="transaction">The transaction to use (optional).</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a list of UIDs that exist in the database, or null if no UIDs are found.
        /// </returns>
        /// <exception cref="Exception">Thrown when an error occurs during query execution.</exception>
        public async Task<List<string>?> CheckIfUIDExistsInDB(string tableName, List<string> uIDs, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            try
            {
                var parameters = new { UIDs = uIDs };
                string sql = $@"SELECT uid AS UID FROM {tableName} WHERE uid IN @UIDs";
                return await ExecuteQueryAsync<string>(sql, parameters, null, connection, transaction);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Checks if the specified UID exists in the specified table in the database.
        /// </summary>
        /// <param name="tableName">The name of the table to check.</param>
        /// <param name="uID">The UID to check for existence.</param>
        /// <param name="connection">The database connection to use (optional). If null, a new connection is created.</param>
        /// <param name="transaction">The transaction to use (optional).</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the UID if it exists in the database, or null if the UID does not exist.
        /// </returns>
        /// <exception cref="Exception">Thrown when an error occurs during query execution.</exception>
        public async Task<string?> CheckIfUIDExistsInDB(string tableName, string uID, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            try
            {
                Dictionary<string, object?> parameters = new()
        {
            { "@UID", uID }

        };
                string sql = $@"SELECT uid AS UID FROM {tableName} WHERE uid = @UID";
                return await ExecuteSingleAsync<string>(sql, parameters, null, connection, transaction);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Deletes records from the specified table in MS SQL Server where the UID matches any of the provided UIDs.
        /// </summary>
        /// <param name="tableName">The name of the table from which to delete records.</param>
        /// <param name="uIDs">The list of UIDs to match for deletion.</param>
        /// <param name="connection">The database connection to use (optional). If null, a new connection is created.</param>
        /// <param name="transaction">The transaction to use (optional).</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the number of rows affected by the deletion.
        /// </returns>
        /// <exception cref="Exception">Thrown when an error occurs during query execution.</exception>
        public async Task<int> DeleteByUID(string tableName, List<string> uIDs, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            try
            {
                Dictionary<string, object?> parameters = new()
        {
            { "@UIDs", uIDs }
        };
                string sql = $@"DELETE FROM {tableName} WHERE uid IN @UIDs";
                return await ExecuteNonQueryAsync(sql, connection, transaction, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        ///  check if a specific table exists in a database
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public async Task<string?> DoesTableExist(string tableName)
        {
            try
            {
                Dictionary<string, object?> parameters = new()
        {
            { "@TableName", tableName }
        };
                string sql = $@"select name from Sys.Tables where Name = @TableName";
                return await ExecuteSingleAsync<string>(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Deletes records from a specified table asynchronously based on a unique identifier (UID).
        /// </summary>
        /// <param name="tableName">The name of the table from which to delete records.</param>
        /// <param name="uID">The unique identifier (UID) value used to identify records to delete.</param>
        /// <param name="connection">Optional: The database connection to use. If not provided, a new connection is created.</param>
        /// <param name="transaction">Optional: The transaction to use with the command, if applicable.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the number of rows affected by the delete operation.</returns>
        /// <exception cref="ArgumentException">Thrown when the provided transaction is not associated with the provided connection.</exception>
        /// <exception cref="Exception">Thrown when an error occurs during delete operation execution.</exception>
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
        /// Appends SQL filter criteria to a StringBuilder based on a list of FilterCriteria objects.
        /// </summary>
        /// <typeparam name="T">The type of model entity for which filter criteria are applied.</typeparam>
        /// <param name="filterCriterias">The list of FilterCriteria objects defining the filter conditions.</param>
        /// <param name="sql">The StringBuilder instance to which SQL filter conditions are appended.</param>
        /// <param name="parameters">A dictionary to store SQL parameter names and values used in filter conditions.</param>
        /// <remarks>
        /// Each FilterCriteria object in the list specifies a condition type (Equal, NotEqual, Like, Is, In, Between) and associated values.
        /// The method dynamically constructs SQL conditions and appends them to the StringBuilder based on the filter criteria.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown when filter criteria values are invalid for a specific condition type (e.g., BETWEEN).</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an unsupported FilterType is encountered.</exception>

        public void AppendFilterCriteria<T>(List<FilterCriteria> filterCriterias, StringBuilder sql, Dictionary<string, object?> parameters) where T : class
        {
            for (int i = 0; i < filterCriterias.Count; i++)
            {
                string paramName = $"filterParam{i}";
                FilterCriteria filterCriteria = filterCriterias[i];

                object? parsedValue = ParseValueToModelFieldDataType<T>(filterCriteria.Name, CommonFunctions.GetStringValue(filterCriteria.Value));

                switch (filterCriteria.Type)
                {
                    case FilterType.NotEqual:
                    case FilterType.Equal:
                        AppendEqualNotEqualCondition(sql, paramName, filterCriteria.Name, parsedValue, filterCriteria.Value, parameters, filterCriteria.Type);
                        break;

                    case FilterType.Like:
                        AppendLikeCondition(sql, paramName, filterCriteria.Name, parsedValue, filterCriteria.Value, parameters);
                        break;

                    case FilterType.Is:
                        sql.Append($"{filterCriteria.Name} IS NULL");
                        break;

                    case FilterType.In:
                        Array inValues = ConvertToArray(filterCriteria.Value);
                        AppendInCondition(sql, paramName, filterCriteria.Name, inValues, filterCriteria.DataType, parameters);
                        break;

                    case FilterType.Between:
                        Array values = ConvertToArray(filterCriteria.Value);
                        if (values is Array array && array.Length >= 2)
                        {
                            AppendBetweenCondition(sql, $"{paramName}_start", $"{paramName}_end", filterCriteria.Name, array.GetValue(0), array.GetValue(1), parameters);
                        }
                        else
                        {
                            throw new ArgumentException($"Invalid values for BETWEEN filter type for column {filterCriteria.Name}");
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (i < filterCriterias.Count - 1)
                {
                    sql.Append(filterCriteria.FilterMode == FilterMode.And ? " AND " : " OR ");
                }
            }
        }

        public object? ParseValueToModelFieldDataType<T>(string fieldName, string fieldValueAsString) where T : class
        {
            PropertyInfo? propertyInfo = _serviceProvider.GetRequiredService<T>().GetType().GetProperty(fieldName);
            if (propertyInfo != null)
            {
                Type fieldType = propertyInfo.PropertyType;
                try
                {
                    if (fieldType == typeof(string))
                    {
                        return fieldValueAsString;
                    }
                    else if (fieldType == typeof(DateTime?))
                    {
                        return DateTime.Parse(fieldValueAsString);
                    }
                    object? parsedValue = Convert.ChangeType(fieldValueAsString, fieldType);
                    if (!fieldType.IsValueType)
                    {
                        parsedValue = fieldType.IsAssignableFrom(parsedValue.GetType()) ? parsedValue : null;
                    }
                    return parsedValue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error converting value for field '{fieldName}': {ex.Message}");
                    return null;
                }
            }
            return null;
        }

        private void AppendBetweenCondition(StringBuilder sql, string paramNameStart, string paramNameEnd, string fieldName, object startValue, object endValue, Dictionary<string, object> parameters)
        {
            if (DateTime.TryParse(CommonFunctions.GetStringValue(startValue), out DateTime startDateTime) && DateTime.TryParse(CommonFunctions.GetStringValue(endValue), out DateTime endDateTime))
            {
                if (startDateTime.TimeOfDay != TimeSpan.Zero || endDateTime.TimeOfDay != TimeSpan.Zero)
                {
                    sql.Append($"{fieldName} BETWEEN @{paramNameStart} AND @{paramNameEnd}");
                    parameters.Add(paramNameStart, startDateTime);
                    parameters.Add(paramNameEnd, endDateTime);
                }
                else
                {
                    sql.Append($"CONVERT(varchar, {fieldName}, 23) BETWEEN @{paramNameStart} AND @{paramNameEnd}");
                    parameters.Add(paramNameStart, startDateTime.Date.ToString("yyyy-MM-dd"));
                    parameters.Add(paramNameEnd, endDateTime.Date.ToString("yyyy-MM-dd"));
                }
            }
            else if (TimeSpan.TryParse(CommonFunctions.GetStringValue(startValue), out TimeSpan startTimeSpan) && TimeSpan.TryParse(CommonFunctions.GetStringValue(endValue), out TimeSpan endTimeSpan))
            {
                sql.Append($"{fieldName} BETWEEN @{paramNameStart} AND @{paramNameEnd}");
                parameters.Add(paramNameStart, startTimeSpan.ToString());
                parameters.Add(paramNameEnd, endTimeSpan.ToString());
            }
        }

        private Array ConvertToArray(object value)
        {
            if (value is IEnumerable enumerable && !(value is string))
            {
                return enumerable.Cast<object>().ToArray();
            }
            else if (value != null && value.GetType().IsArray)
            {
                return (Array)value;
            }
            else
            {
                return new object[] { value };
            }
        }

        private void AppendInCondition(StringBuilder sql, string paramName, string columnName, Array rawValue, Type dataType, Dictionary<string, object> parameters)
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

        private void AppendInConditionForArray(StringBuilder sql, string paramName, string columnName, Array rawValue, Type dataType, Dictionary<string, object> parameters)
        {
            List<string> inParam = new();
            int j = 0;

            foreach (object? value in rawValue)
            {
                string paramKey = $"{paramName}_in{j++}";
                inParam.Add($"@{paramKey}");

                object parsedValue = Convert.ChangeType(value, dataType);
                parameters.Add(paramKey, parsedValue);
            }

            sql.Append($"{columnName} IN ({string.Join(", ", inParam)})");
        }

        private void AppendEqualNotEqualCondition(StringBuilder sql, string paramName, string fieldName, object? parsedValue, object filterValue, Dictionary<string, object> parameters, FilterType filterCriteriaType)
        {
            string operatorString = filterCriteriaType == FilterType.NotEqual ? "!=" : "=";

            if (parsedValue != null && parsedValue.GetType() == typeof(DateTime))
            {
                DateTime dateTimeValue = (DateTime)parsedValue;
                if (dateTimeValue.TimeOfDay != TimeSpan.Zero)
                {
                    sql.Append($"{fieldName} {operatorString} @{paramName}");
                    parameters.Add(paramName, dateTimeValue);
                }
                else
                {
                    sql.Append($"CONVERT(varchar, {fieldName}, 23) {operatorString} @{paramName}");
                    parameters.Add(paramName, dateTimeValue.Date.ToString("yyyy-MM-dd"));
                }
            }
            else if (parsedValue != null && parsedValue.GetType() != typeof(string))
            {
                sql.Append($"{fieldName} {operatorString} @{paramName}");
                parameters.Add(paramName, parsedValue);
            }
            else
            {
                sql.Append($"{fieldName} {operatorString} @{paramName}");
                parameters.Add(paramName, filterValue);
            }
        }

        private void AppendLikeCondition(StringBuilder sql, string paramName, string fieldName, object? parsedValue, object filterValue, Dictionary<string, object> parameters)
        {
            string likeOperator = "LIKE";

            if (parsedValue != null && parsedValue.GetType() == typeof(DateTime))
            {
                DateTime dateTimeValue = (DateTime)parsedValue;
                if (dateTimeValue.TimeOfDay != TimeSpan.Zero)
                {
                    sql.Append($"CONVERT(varchar, {fieldName}, 120) {likeOperator} '%' + @{paramName} + '%'");
                    parameters.Add(paramName, dateTimeValue.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                }
                else
                {
                    sql.Append($"CONVERT(varchar, {fieldName}, 23) {likeOperator} '%' + @{paramName} + '%'");
                    parameters.Add(paramName, dateTimeValue.Date.ToString("yyyy-MM-dd"));
                }
            }
            else if (parsedValue != null && parsedValue.GetType() != typeof(string))
            {
                sql.Append($"{fieldName} {likeOperator} '%' + @{paramName} + '%'");
                parameters.Add(paramName, parsedValue);
            }
            else
            {
                sql.Append($"{fieldName} {likeOperator} '%' + @{paramName} + '%'");
                parameters.Add(paramName, filterValue);
            }
        }


        public void AppendSortCriteria(List<SortCriteria> sortCriterias, StringBuilder sql)
        {
            for (var i = 0; i < sortCriterias.Count; i++)
            {
                var sortCriteria = sortCriterias[i];

                sql.Append($"[{sortCriteria.SortParameter}] {(sortCriteria.Direction == SortDirection.Asc ? "ASC" : "DESC")}");

                if (i < sortCriterias.Count - 1)
                {
                    sql.Append(", ");
                }
            }
        }










        //public async Task<int> ExecuteNonQueryAsync(string sql, IDictionary<string, object> parameters = null)
        //{
        //    using (var conn = new OracleConnection(_connectionString))
        //    using (var cmd = new SqlCommand(sql, conn))
        //    {
        //        if (parameters != null)
        //        {
        //            foreach (var parameter in parameters)
        //            {
        //                cmd.Parameters.AddWithValue(parameter.Key, (parameter.Value == null) ? DBNull.Value : parameter.Value);
        //            }
        //        }

        //        await conn.OpenAsync();
        //        return await cmd.ExecuteNonQueryAsync();
        //    }
        //}
        //public async Task<int> ExecuteNonQueryAsync(string sql, OracleConnection connection, SqlTransaction transaction, IDictionary<string, object> parameters = null)
        //{
        //    using (var cmd = new SqlCommand(sql, connection, transaction))
        //    {
        //        if (parameters != null)
        //        {
        //            foreach (var parameter in parameters)
        //            {
        //                cmd.Parameters.AddWithValue(parameter.Key, (parameter.Value == null) ? DBNull.Value : parameter.Value);
        //            }
        //        }

        //        return await cmd.ExecuteNonQueryAsync();
        //    }
        //}
        //public async Task<T> ExecuteScalarAsync<T>(string sql, IDictionary<string, object> parameters = null)
        //{
        //    using (var conn = new OracleConnection(_connectionString))
        //    using (var cmd = new SqlCommand(sql, conn))
        //    {
        //        if (parameters != null)
        //        {
        //            foreach (var parameter in parameters)
        //            {
        //                cmd.Parameters.AddWithValue(parameter.Key, (parameter.Value == null) ? DBNull.Value : parameter.Value);
        //            }
        //        }

        //        await conn.OpenAsync();
        //        var result = await cmd.ExecuteScalarAsync();

        //        if (result is DBNull || result == null)
        //        {
        //            return default;
        //        }

        //        return (T)Convert.ChangeType(result, typeof(T));
        //    }
        //}
        //public async Task<List<T>> ExecuteQueryAsync<T>(string sql, IDictionary<string, object> parameters = null, Type type = null)
        //{

        //    using (var conn = new OracleConnection(_connectionString))
        //    using (var cmd = new SqlCommand(sql, conn))
        //    {
        //        if (parameters != null)
        //        {
        //            foreach (var parameter in parameters)
        //            {
        //                cmd.Parameters.AddWithValue(parameter.Key, (parameter.Value == null) ? DBNull.Value : parameter.Value);
        //            }
        //        }

        //        await conn.OpenAsync();

        //        var resultList = new List<T>();
        //        using (var reader = await cmd.ExecuteReaderAsync())
        //        {
        //            while (await reader.ReadAsync())
        //            {
        //                //var result =Activator.CreateInstance<T>();
        //                object? result = null;
        //                if (type != null)
        //                {
        //                    result = Activator.CreateInstance(type);
        //                }
        //                else
        //                {
        //                    //result = Activator.CreateInstance<T>();
        //                    result = _serviceProvider.CreateInstance<T>();
        //                }

        //                foreach (var property in result.GetType().GetProperties())
        //                {
        //                    try
        //                    {
        //                        if (!reader.IsDBNull(reader.GetOrdinal(property.Name)))
        //                        {
        //                            var value = reader.GetValue(reader.GetOrdinal(property.Name));
        //                            property.SetValue(result, value);
        //                        }
        //                    }
        //                    catch (IndexOutOfRangeException)
        //                    {
        //                        // Handle the case when the column does not exist

        //                    }
        //                    catch (Exception)
        //                    {
        //                        // Handle any other exceptions that occur while setting property values
        //                        throw;
        //                    }
        //                }

        //                resultList.Add((T)result);
        //            }
        //        }

        //        return resultList;
        //    }
        //}

        public async Task<DataTable> ExecuteQueryDataTableAsync(string sql, IDictionary<string, object> parameters = null)
        {
            using (var conn = new OracleConnection(_connectionString))
            using (var cmd = new OracleCommand(sql, conn))
            {
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        cmd.Parameters.Add(parameter.Key, (parameter.Value == null) ? DBNull.Value : parameter.Value);
                    }
                }

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var dataTable = new DataTable();
                    dataTable.Load(reader);
                    return dataTable;
                }
            }
        }
        public async Task<DataSet> ExecuteQueryDataSetAsync(string sql, IDictionary<string, object> parameters = null)
        {
            using (var conn = new OracleConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new OracleCommand(sql, conn))
                {
                    if (parameters != null)
                    {
                        foreach (var parameter in parameters)
                        {
                            cmd.Parameters.Add(parameter.Key, (parameter.Value == null) ? DBNull.Value : parameter.Value);
                        }
                    }

                    var dataSet = new DataSet();
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        da.Fill(dataSet);
                    }

                    return dataSet;
                }
            }
        }
        //public async Task<T> ExecuteSingleAsync<T>(string sql, IDictionary<string, object> parameters = null, Type type = null)
        //{
        //    using (var conn = new OracleConnection(_connectionString))
        //    using (var cmd = new SqlCommand(sql, conn))
        //    {
        //        if (parameters != null)
        //        {
        //            foreach (var parameter in parameters)
        //            {
        //                cmd.Parameters.AddWithValue(parameter.Key, (parameter.Value == null) ? DBNull.Value : parameter.Value);
        //            }
        //        }

        //        await conn.OpenAsync();
        //        using (var reader = await cmd.ExecuteReaderAsync())
        //        {
        //            if (await reader.ReadAsync())
        //            {
        //                //var result = CreateInstance<T>();// Activator.CreateInstance<T>();
        //                object? result = null;
        //                if (type != null)
        //                {
        //                    result = Activator.CreateInstance(type);
        //                }
        //                else
        //                {
        //                    //result = Activator.CreateInstance<T>();
        //                    result = _serviceProvider.CreateInstance<T>();
        //                }
        //                foreach (var property in result.GetType().GetProperties())
        //                {
        //                    try
        //                    {
        //                        if (!reader.IsDBNull(reader.GetOrdinal(property.Name)))
        //                        {
        //                            var value = reader.GetValue(reader.GetOrdinal(property.Name));
        //                            property.SetValue(result, value);
        //                        }
        //                    }
        //                    catch (IndexOutOfRangeException)
        //                    {
        //                        // Handle the case when the column does not exist

        //                    }
        //                    catch (Exception)
        //                    {
        //                        // Handle any other exceptions that occur while setting property values
        //                        throw;
        //                    }
        //                }

        //                return (T)result;
        //            }
        //        }

        //        return default;
        //    }
        //}


    }
}

