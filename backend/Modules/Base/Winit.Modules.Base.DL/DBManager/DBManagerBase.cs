using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using System.Transactions;
using System.Data.Common;
using Elasticsearch.Net;

namespace Winit.Modules.Base.DL.DBManager
{
    public class DBManagerBase
    {
        public readonly IServiceProvider _serviceProvider;
        protected readonly string _connectionString = string.Empty;

        private IAppConfig _appConfig { get; set; }
        private IConfiguration _config { get; }

        public DBManagerBase(IServiceProvider serviceProvider, IConfiguration config, string connectionStringName)
        {
            _serviceProvider = serviceProvider;
            _config = config;
            if (connectionStringName == ConnectionStringName.SQLite)
            {
                _appConfig = serviceProvider.GetRequiredService<IAppConfig>();
             
                _connectionString = GetSqliteConnectionString();
            }
            else
            {
                _connectionString = config.GetConnectionString(connectionStringName) ?? string.Empty;
            }
        }

        protected SqlConnection CreateConnection(string connectionStringName)
        {
            string connection = _config[connectionStringName];
            return new SqlConnection(_config.GetConnectionString(connectionStringName) ?? string.Empty);
        }

        protected SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
        //protected OracleConnection OracleConnection()
        //{
        //    return new OracleConnection(_connectionString);
        //}


        protected NpgsqlConnection PostgreConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        protected SqliteConnection SqliteConnection()
        {
            return new SqliteConnection(_connectionString);
        }

        protected SqliteConnection SqliteConnection(string connectionString)
        {
            return new SqliteConnection(connectionString);
        }

        protected void ValidateTransaction(IDbConnection? connection, IDbTransaction? transaction)
        {
            if (transaction != null)
            {
                // Check if the transaction's connection matches the command's connection
                if (transaction.Connection != connection)
                {
                    throw new InvalidOperationException("Transaction is associated with a different connection.");
                }
            }
        }

        protected void RollbackTransaction(IDbTransaction? transaction)
        {
            try
            {
                // Rollback the transaction if it hasn't been completed
                if (transaction?.Connection != null)
                {
                    transaction?.Rollback();
                }
            }
            catch (Exception rollbackEx)
            {
                // Log or handle the rollback exception
                Console.WriteLine($"Rollback exception: {rollbackEx.Message}");
            }
        }

        public DBManagerBase(IServiceProvider serviceProvider, string connectionStringName)
        {
            _serviceProvider = serviceProvider;
            if (connectionStringName == ConnectionStringName.SQLite)
            {
                _appConfig = serviceProvider.GetRequiredService<IAppConfig>();
                //string sourceFileName = "WINITSQLite.db";
                //string personalFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                //string connectionString = Path.Combine(personalFolderPath, sourceFileName);
                _connectionString = GetSqliteConnectionString();
            }
        }

        string GetSqliteConnectionString()
        {
            string
                appBaseDirectory = Path.Combine(_appConfig.BaseFolderPath, "DB"); 
            string fileName = "WINITSQLite.db";
            return $"Data Source={Path.Combine(appBaseDirectory, fileName)};Mode=ReadWrite;Pooling=False;";
        }

        public string GetSqliteConnectionString(string sqliteFilePath)
        {
            return $"Data Source={sqliteFilePath};Mode=ReadWrite;Pooling=False;";
        }

        /*
        protected T CreateInstance<T>()
        {
            return _serviceProvider.GetRequiredService<T>();
        }
        protected List<T> SerializeDatasetToList<T>(System.Data.DataSet set)
        {
            if (!ValidateDataSet(set))
                return new List<T>();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(Newtonsoft.Json.JsonConvert.SerializeObject(set.Tables[0]),
                new Newtonsoft.Json.JsonSerializerSettings()
                {
                    //DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.IgnoreAndPopulate,
                    NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                    MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore,
                });
        }
        protected List<T> SerializeDataRowCollectionToList<T>(System.Data.DataRowCollection rows)
        {
            if (!ValidateDataRowCollection(rows))
                return new List<T>();
            System.Data.DataTable dt = new System.Data.DataTable();
            foreach (System.Data.DataRow dr in rows)
            {
                dt.ImportRow(dr);
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(Newtonsoft.Json.JsonConvert.SerializeObject(dt),
                new Newtonsoft.Json.JsonSerializerSettings()
                {
                    //DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.IgnoreAndPopulate
                    NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                    MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore
                });
        }
        protected T SerializeDataRowToObject<T>(System.Data.DataRow rows)
        {
            if (rows == null)
                return default(T);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Newtonsoft.Json.JsonConvert.SerializeObject(rows),
                new Newtonsoft.Json.JsonSerializerSettings()
                {
                    //DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.IgnoreAndPopulate
                    NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                    MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore
                });
        }
        */
        public T ConvertDataTableToObject1<T>(DataRow row, IFactory factory = null,
            Dictionary<string, string> columnMappings = null)
        {
            T result;

            if (factory != null)
            {
                result = (T)factory.CreateInstance();
            }
            else
            {
                result = Activator.CreateInstance<T>();
            }

            foreach (DataColumn column in row.Table.Columns)
            {
                string propertyName;
                if (columnMappings != null &&
                    columnMappings.TryGetValue(column.ColumnName, out string aliasPropertyName))
                {
                    propertyName = aliasPropertyName;
                }
                else
                {
                    propertyName = column.ColumnName;
                }

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

                    else
                    {
                        // For non-DateTime properties, set the value directly
                        property.SetValue(result, row[column]);
                    }
                }
            }

            return result;
        }

        public T ConvertDataTableToObject_CI<T>(DataRow row, Dictionary<string, string> columnMappings = null,
            Type type = null)
        {
            T result;

            if (type != null)
            {
                result = (T)Activator.CreateInstance(type);
            }
            else
            {
                result = Activator.CreateInstance<T>();
            }

            foreach (DataColumn column in row.Table.Columns)
            {
                string propertyName;
                if (columnMappings != null &&
                    columnMappings.TryGetValue(column.ColumnName, out string aliasPropertyName))
                {
                    propertyName = aliasPropertyName;
                }
                else
                {
                    propertyName = column.ColumnName;
                }

                PropertyInfo property = result.GetType().GetProperty(propertyName);
                if (property != null && row[column] != DBNull.Value)
                {
                    if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                    {
                        if (DateTime.TryParse(row[column].ToString(), out DateTime dateTimeValue))
                        {
                            if (property.PropertyType == typeof(DateTime?))
                            {
                                property.SetValue(result, (DateTime?)dateTimeValue);
                            }
                            else
                            {
                                property.SetValue(result, dateTimeValue);
                            }
                        }
                    }
                    else if (property.PropertyType == typeof(int?))
                    {
                        if (int.TryParse(row[column].ToString(), out int intV))
                        {
                            property.SetValue(result, (int?)intV);
                        }
                    }
                    else
                    {
                        property.SetValue(result, Convert.ChangeType(row[column], property.PropertyType));
                    }
                }
            }

            return result;
        }

        public T ConvertDataTableToObject<T>(DataRow row, Dictionary<string, string> columnMappings = null,
            Type type = null)
        {
            T result;

            if (type != null)
            {
                result = (T)Activator.CreateInstance(type);
            }
            else
            {
                result = Activator.CreateInstance<T>();
            }

            var propertyInfos = result.GetType().GetProperties()
                .ToDictionary(p => p.Name.ToLower(), p => p);

            foreach (DataColumn column in row.Table.Columns)
            {
                string propertyName;
                if (columnMappings != null &&
                    columnMappings.TryGetValue(column.ColumnName, out string aliasPropertyName))
                {
                    propertyName = aliasPropertyName;
                }
                else
                {
                    propertyName = column.ColumnName;
                }

                if (propertyInfos.TryGetValue(propertyName.ToLower(), out PropertyInfo property))
                {
                    if (property != null && row[column] != DBNull.Value)
                    {
                        if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                        {
                            if (DateTime.TryParse(row[column].ToString(), out DateTime dateTimeValue))
                            {
                                if (property.PropertyType == typeof(DateTime?))
                                {
                                    property.SetValue(result, (DateTime?)dateTimeValue);
                                }
                                else
                                {
                                    property.SetValue(result, dateTimeValue);
                                }
                            }
                        }
                        else if (property.PropertyType == typeof(int?))
                        {
                            if (int.TryParse(row[column].ToString(), out int intV))
                            {
                                property.SetValue(result, (int?)intV);
                            }
                        }
                        else if (property.PropertyType == typeof(decimal?))
                        {
                            if (decimal.TryParse(row[column].ToString(), out decimal decimalV))
                            {
                                property.SetValue(result, (decimal?)decimalV);
                            }
                        }
                        else
                        {
                            property.SetValue(result, Convert.ChangeType(row[column], property.PropertyType));
                        }
                    }
                }
            }

            return result;
        }

        public T ConvertDataTableToObjectBool<T>(DataRow row, Dictionary<string, string> columnMappings = null,
            Type type = null)
        {
            T result;

            if (type != null)
            {
                result = (T)Activator.CreateInstance(type);
            }
            else
            {
                result = Activator.CreateInstance<T>();
            }

            var propertyInfos = result.GetType().GetProperties()
                .ToDictionary(p => p.Name.ToLower(), p => p);

            foreach (DataColumn column in row.Table.Columns)
            {
                string propertyName;
                if (columnMappings != null &&
                    columnMappings.TryGetValue(column.ColumnName, out string aliasPropertyName))
                {
                    propertyName = aliasPropertyName;
                }
                else
                {
                    propertyName = column.ColumnName;
                }

                if (propertyInfos.TryGetValue(propertyName.ToLower(), out PropertyInfo property))
                {
                    if (property != null && row[column] != DBNull.Value)
                    {
                        if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                        {
                            if (DateTime.TryParse(row[column].ToString(), out DateTime dateTimeValue))
                            {
                                if (property.PropertyType == typeof(DateTime?))
                                {
                                    property.SetValue(result, (DateTime?)dateTimeValue);
                                }
                                else
                                {
                                    property.SetValue(result, dateTimeValue);
                                }
                            }
                        }
                        else if (property.PropertyType == typeof(int?) || property.PropertyType == typeof(int))
                        {
                            if (int.TryParse(row[column].ToString(), out int intV))
                            {
                                property.SetValue(result, property.PropertyType == typeof(int?) ? (int?)intV : intV);
                            }
                        }
                        else if (property.PropertyType == typeof(bool?) || property.PropertyType == typeof(bool))
                        {
                            if (bool.TryParse(row[column].ToString(), out bool boolV))
                            {
                                property.SetValue(result,
                                    property.PropertyType == typeof(bool?) ? (bool?)boolV : boolV);
                            }
                        }
                        else if (property.PropertyType == typeof(decimal?) || property.PropertyType == typeof(decimal))
                        {
                            if (decimal.TryParse(row[column].ToString(), out decimal decimalV))
                            {
                                property.SetValue(result,
                                    property.PropertyType == typeof(decimal?) ? (decimal?)decimalV : decimalV);
                            }
                        }
                        else
                        {
                            property.SetValue(result, Convert.ChangeType(row[column], property.PropertyType));
                        }
                    }
                }
            }

            return result;
        }


        #region Utility methods

        /// <summary>
        /// Escapes an input string for database processing, that is, 
        /// surround it with quotes and change any quote in the string to 
        /// two adjacent quotes (i.e. escape it). 
        /// If input string is null or empty a NULL string is returned.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>Escaped output string.</returns>
        public static string Escape(string s)
        {
            if (String.IsNullOrEmpty(s))
                return "NULL";
            else
                return "'" + s.Trim().Replace("'", "''") + "'";
        }

        /// <summary>
        /// Escapes an input string for database processing, that is, 
        /// surround it with quotes and change any quote in the string to 
        /// two adjacent quotes (i.e. escape it). 
        /// Also trims string at a given maximum length.
        /// If input string is null or empty a NULL string is returned.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <param name="maxLength">Maximum length of output string.</param>
        /// <returns>Escaped output string.</returns>
        public static string Escape(string s, int maxLength)
        {
            if (String.IsNullOrEmpty(s))
                return "NULL";
            else
            {
                s = s.Trim();
                if (s.Length > maxLength) s = s.Substring(0, maxLength - 1);
                return "'" + s.Trim().Replace("'", "''") + "'";
            }
        }

        /// <summary>
        /// converts an object to double value
        /// </summary>
        /// <param name="value">object</param>
        /// <returns>double</returns>
        public static double ToDouble(object value)
        {
            double retValue = 0;

            if (value != DBNull.Value)
            {
                double.TryParse(value.ToString(), out retValue);
            }

            return retValue;
        }

        /// <summary>
        /// converts an object to decimal
        /// </summary>
        /// <param name="value">object</param>
        /// <returns>decimal</returns>
        public static decimal ToDecimal(object value)
        {
            decimal retValue = 0;

            if (value != DBNull.Value)
            {
                decimal.TryParse(value.ToString(), out retValue);
            }

            return retValue;
        }

        /// <summary>
        /// converts an object to float
        /// </summary>
        /// <param name="value">object</param>
        /// <returns>float</returns>
        public static float ToFloat(object value)
        {
            float retValue = 0;

            if (value != DBNull.Value)
            {
                float.TryParse(value.ToString(), out retValue);
            }

            return retValue;
        }

        /// <summary>
        ///  converts an object to integer value
        /// </summary>
        /// <param name="value">object</param>
        /// <returns>int</returns>
        public static int ToInteger(object value)
        {
            int retValue = 0;

            if (value != DBNull.Value)
            {
                int.TryParse(value.ToString(), out retValue);
            }

            return retValue;
        }

        /// <summary>
        ///  converts an object to long value
        /// </summary>
        /// <param name="value">object</param>
        /// <returns>System.Int64</returns>
        public static long ToLong(object value)
        {
            long retValue = 0;

            if (value != DBNull.Value)
            {
                long.TryParse(value.ToString(), out retValue);
            }

            return retValue;
        }

        /// <summary>
        ///  converts an object to string value
        /// </summary>
        /// <param name="value">object</param>
        /// <returns>string</returns>
        public static string ToString(object value)
        {
            string retValue = "";

            if (value != DBNull.Value)
            {
                retValue = value.ToString();
            }

            return retValue;
        }

        /// <summary>
        ///  converts an object to boolean value
        /// </summary>
        /// <param name="value">object</param>
        /// <returns>bool</returns>
        public static bool ToBoolean(object value)
        {
            bool retValue = false;

            if (value != DBNull.Value)
            {
                try
                {
                    value = Convert.ToBoolean(value);
                }
                catch (Exception ex)
                {
                }

                bool.TryParse(value.ToString(), out retValue);
            }

            return retValue;
        }

        /// <summary>
        ///  converts an object to datetime value
        /// </summary>
        /// <param name="value">object</param>
        /// <returns>System.DateTime</returns>
        public static DateTime ToDateTime(object value)
        {
            DateTime retValue = new DateTime();

            if (value != DBNull.Value)
            {
                DateTime.TryParse(value.ToString(), out retValue);
            }

            return retValue;
        }

        /// <summary>
        ///  converts an object to bigint value
        /// </summary>
        /// <param name="value">object</param>
        /// <returns>Int32</returns>
        public static Int64 ToBigInteger(object value)
        {
            int retValue = 0;

            if (value != DBNull.Value)
            {
                try
                {
                    retValue = int.Parse(value.ToString());
                }
                catch (Exception ex)
                {
                }
            }

            return retValue;
        }

        #endregion
    }
}


/* public T ConvertDataTableToObject<T>(DataRow row, Dictionary<string, string> columnMappings = null, Type type = null)
     {
         T result;

         if (type != null)
         {
             result = (T)Activator.CreateInstance(type);
         }
         else
         {
             result = Activator.CreateInstance<T>();
         }

         foreach (DataColumn column in row.Table.Columns)
         {
             string propertyName;
             if (columnMappings != null && columnMappings.TryGetValue(column.ColumnName, out string aliasPropertyName))
             {
                 propertyName = aliasPropertyName;
             }
             else
             {
                 propertyName = column.ColumnName;
             }

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
                 else
                 {
                     // For non-DateTime properties, set the value directly
                     property.SetValue(result, Convert.ChangeType(row[column], property.PropertyType));
                 }
             }
         }

         return result;
     }*/