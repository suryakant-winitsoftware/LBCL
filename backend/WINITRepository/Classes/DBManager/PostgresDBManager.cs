using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using WINITSharedObjects.Enums;

namespace WINITRepository.Classes.DBManager
{
    public class PostgresDBManager<T>
    {
        private readonly string _connectionString;

        public PostgresDBManager(string connectionString)
        {
            _connectionString = connectionString;
        }
        /// <summary>
        /// This method is used to execute SQL statements that do not return any rows, such as INSERT, UPDATE, and DELETE statements.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<int> ExecuteNonQueryAsync(string sql, IDictionary<string, object> parameters = null)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.CommandTimeout = 0;
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
                    }
                }

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync();
            }
        }
        /// <summary>
        /// This method is used to execute SQL statements that return a single value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<T> ExecuteScalarAsync<T>(string sql, IDictionary<string, object> parameters = null)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.CommandTimeout = 0;
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
                    }
                }

                await conn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();

                if (result is DBNull || result == null)
                {
                    return default(T);
                }

                return (T)Convert.ChangeType(result, typeof(T));
            }
        }
        /// <summary>
        /// Executes a SQL query and returns a list of results as objects of type T
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<List<T>> ExecuteQueryAsync(string sql, IDictionary<string, object> parameters = null)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.CommandTimeout = 0;
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
                    }
                }

                await conn.OpenAsync();

                var resultList = new List<T>();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var result = Activator.CreateInstance<T>();
                        foreach (var property in result.GetType().GetProperties())
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal(property.Name)))
                            {
                                var value = reader.GetValue(reader.GetOrdinal(property.Name));
                                property.SetValue(result, value);
                            }
                        }

                        resultList.Add(result);
                    }
                }

                return resultList;
            }
        }

        /// <summary>
        /// Executes a SQL query and returns a DataTable with the results
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<DataTable> ExecuteQueryDataTableAsync(string sql, IDictionary<string, object> parameters = null)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.CommandTimeout = 0;
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
                    var dataTable = new DataTable();
                    dataTable.Load(reader);
                    return dataTable;
                }
            }
        }

        /// <summary>
        /// Executes a SQL query and returns a single object of type T
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<T> ExecuteSingleAsync(string sql, IDictionary<string, object> parameters = null)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.CommandTimeout = 0;
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
                        var result = Activator.CreateInstance<T>();
                        foreach (var property in result.GetType().GetProperties())
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal(property.Name)))
                            {
                                var value = reader.GetValue(reader.GetOrdinal(property.Name));
                                property.SetValue(result, value);
                            }
                        }

                        return result;
                    }

                    return default(T);
                }
            }
        }

        //ramana added
        public void AppendFilterCriteria(List<FilterCriteria> filterCriterias, StringBuilder sql, Dictionary<string, object> parameters)
        {
            for (var i = 0; i < filterCriterias.Count; i++)
            {
                var paramName = $"filterParam{i}";
                var filterCriteria = filterCriterias[i];

                sql.Append($"{filterCriteria.Name} ");

                switch (filterCriteria.Type)
                {
                    case FilterType.NotEqual:
                        if (filterCriteria.Value is int)
                        {
                            sql.Append($"!= :{paramName}"); 
                        }
                        else
                        {
                            sql.Append($"!= :{paramName}::text"); 
                        }
                        break;
                    case FilterType.Equal:
                        if (filterCriteria.Value is int)
                        {
                            sql.Append($"= :{paramName}"); 
                        }
                        else
                        {
                            sql.Append($"= :{paramName}::text"); 
                        }
                        break;
                    case FilterType.Like:
                        sql.Append($"ILIKE '%' || :{paramName} || '%'"); 
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (i < filterCriterias.Count - 1)
                {
                    sql.Append(" AND ");
                }

                parameters.Add(paramName, filterCriteria.Value);
            }
        }


        public void AppendSortCriteria(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, StringBuilder sql)
        {
            for (var i = 0; i < sortCriterias.Count; i++)
            {
                var sortCriteria = sortCriterias[i];

                sql.Append($"\"{sortCriteria.SortParameter}\" {(sortCriteria.Direction == SortDirection.Asc ? "ASC" : "DESC")}");

                if (i < sortCriterias.Count - 1)
                {
                    sql.Append(", ");
                }
            }
        }

        //public void AppendFilterCriteria(List<FilterCriteria> filterCriterias, StringBuilder sql, Dictionary<string, object> parameters)
        //{
        //    for (var i = 0; i < filterCriterias.Count; i++)
        //    {
        //       // var paramName = $"filterParam{i}";
        //        var paramName = $"filterParam{i}";
        //        var filterCriteria = filterCriterias[i];

        //        sql.Append($"{filterCriteria.Name} ");

        //        switch (filterCriteria.Type)
        //        {
        //            case FilterType.NotEqual:
        //                sql.Append($"!= {paramName}");
        //                break;
        //            case FilterType.Equal:
        //                sql.Append($"= {paramName}");
        //                break;
        //            case FilterType.Like:
        //                sql.Append($"ILIKE '%' || {paramName} || '%'");
        //                break;
        //            default:
        //                throw new ArgumentOutOfRangeException();
        //        }

        //        if (i < filterCriterias.Count - 1)
        //        {
        //            sql.Append(" AND ");
        //        }

        //        parameters.Add(paramName, filterCriteria.Value);
        //    }
        //}



    }
}
