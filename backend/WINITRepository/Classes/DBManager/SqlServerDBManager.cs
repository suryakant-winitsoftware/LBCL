using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.Text;
using WINITSharedObjects.Enums;

namespace WINITRepository.Classes.DBManager
{
    public class SqlServerDBManager<T>
    {
        private readonly string _connectionString;

        public SqlServerDBManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, IDictionary<string, object> parameters = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
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

        public async Task<T> ExecuteScalarAsync<T>(string sql, IDictionary<string, object> parameters = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
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

        public async Task<List<T>> ExecuteQueryAsync(string sql, IDictionary<string, object> parameters = null)
        {

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
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
                                try
                                {
                                    var value = reader.GetValue(reader.GetOrdinal(property.Name));
                                    property.SetValue(result, value);
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }

                        resultList.Add(result);
                    }
                }

                return resultList;
            }
        }

        public async Task<DataTable> ExecuteQueryDataTableAsync(string sql, IDictionary<string, object> parameters = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
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
                    var dataTable = new DataTable();
                    dataTable.Load(reader);
                    return dataTable;
                }
            }
        }



        //ramana 

        public async Task<DataSet> ExecuteQueryDataSetAsync(string sql, IDictionary<string, object> parameters = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
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
                    var dataSet = new DataSet();
                    var dataTable = new DataTable();
                    dataTable.Load(reader);
                    dataSet.Tables.Add(dataTable);
                    return dataSet;
                }
            }
        }


        public async Task<T> ExecuteSingleAsync(string sql, IDictionary<string, object> parameters = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
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
                }

                return default(T);
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
                        sql.Append($"!= @{paramName}");
                        break;
                    case FilterType.Equal:
                        sql.Append($"= @{paramName}");
                        break;
                    case FilterType.Like:
                        sql.Append($"LIKE '%' + @{paramName} + '%'");
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

                sql.Append($"[{sortCriteria.SortParameter}] {(sortCriteria.Direction == SortDirection.Asc ? "ASC" : "DESC")}");

                if (i < sortCriterias.Count - 1)
                {
                    sql.Append(", ");
                }
            }
        }
    }
}
