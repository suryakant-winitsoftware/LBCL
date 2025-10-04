using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Winit.Modules.Base.DL.DBManager;

namespace WinIT.mSFA.Shared.Database
{
    public abstract class SqliteDBManager1 : DBManagerBase
    {
        public static string _baseFolderPath = Environment.OSVersion.Platform == PlatformID.Unix ? Environment.GetFolderPath(Environment.SpecialFolder.Personal) : Path.Combine(Environment.CurrentDirectory, "Data");
        protected abstract string SqliteFileName { get; }
        static Dictionary<string, SqliteConnection> _mutitonconnection = new Dictionary<string, SqliteConnection>();
        //Comments: SqlLite - Single thread transaction is supported. 
        static Dictionary<string, (SqliteConnection Connection, SqliteTransaction Transaction)> _multitonconnectionwithtransaction = new Dictionary<string, (SqliteConnection Connection, SqliteTransaction Transaction)>();
        protected bool _withtransaction = false;
        protected static bool _intransaction = false;
        string _connectionString = null;
        int? _timeout;
        //protected bool IsDbExist { get { return System.IO.File.Exists(_baseFilePath); } }
        /// <summary>
        /// SqliteDBManager Layer constructor
        /// When transaction enabled, its developer responsibility to call Commit or Rollback
        /// Without commit or rollback, this would lead to incosistent behaviour
        /// </summary>
        /// <param name="transaction">true - enable transaction, false (default) - no transaction</param>
        public SqliteDBManager1(bool transaction, int? timeout)
        {
            _withtransaction = transaction;
            _timeout = timeout;
            //_connectionString = $"URI=file:{Path.Combine(_baseFolderPath, SqliteFileName)}";
        }
        public SqliteDBManager1(bool transaction) : this(transaction, null) { }
        string GetConnectionString()
        {
            //return string.IsNullOrEmpty(_connectionString) ? $"Data Source={Path.Combine(_baseFolderPath, SqliteFileName)};Version=3;Pooling=True;Max Pool Size=100;" : throw new ApplicationException("Invalid coonnection string.");
            return string.IsNullOrEmpty(_connectionString) ? $"Data Source={Path.Combine(_baseFolderPath, SqliteFileName)};" : throw new ApplicationException("Invalid coonnection string.");
        }
        protected async Task<SqliteConnection> GetTransactionConnectionAsync()
        {
            _intransaction = true;
            SqliteConnection _connectionwithtransaction = null;
            if (_multitonconnectionwithtransaction.ContainsKey(SqliteFileName))
                _connectionwithtransaction = _multitonconnectionwithtransaction[SqliteFileName].Connection;
            if (_connectionwithtransaction == null)
            {
                _connectionwithtransaction = new SqliteConnection(GetConnectionString());
                if (_connectionwithtransaction.State != System.Data.ConnectionState.Open)
                    await _connectionwithtransaction.OpenAsync();
                if (_multitonconnectionwithtransaction.ContainsKey(SqliteFileName))
                    _multitonconnectionwithtransaction.Remove(SqliteFileName);
                _multitonconnectionwithtransaction.Add(SqliteFileName, (_connectionwithtransaction, _connectionwithtransaction.BeginTransaction()));
            }
            return _connectionwithtransaction;
        }
        //TO DO: concurrency issue - but ok with sqllite - Still to validate
        protected async Task<SqliteConnection> GetDefaultConnectionAsync()
        {
            //SqliteConnection _connection = new SqliteConnection(GetConnectionString());
            //await _connection.OpenAsync();
            int secs = 0;
            while (_intransaction)
            {
                if (secs > 60)
                    throw new ApplicationException("Transaction taking laonger than expected, please close the transaction");
                secs++;
                await Task.Delay(1000);
            }
            SqliteConnection _connection = null;
            if (_mutitonconnection.ContainsKey(SqliteFileName))
                _connection = _mutitonconnection[SqliteFileName];
            if (_connection == null)
            {
                _connection = new SqliteConnection(GetConnectionString());
                if (_connection.State != System.Data.ConnectionState.Open)
                    await _connection.OpenAsync();
                if (_mutitonconnection.ContainsKey(SqliteFileName))
                    _mutitonconnection[SqliteFileName] = _connection;
                else
                    _mutitonconnection.Add(SqliteFileName, _connection);
            }

            return _connection;
        }
        protected async Task<SqliteConnection> GetConnectionAsync()
        {
            if (_withtransaction)
                return await GetTransactionConnectionAsync();
            else
                return await GetDefaultConnectionAsync();
        }
        static object _lock = new object();
        protected SqliteConnection GetConnection()
        {
            SqliteConnection conntection = GetConnectionAsync().GetAwaiter().GetResult();
            if (conntection != null && conntection.State != ConnectionState.Open)
            {
                conntection = GetConnectionAsync().GetAwaiter().GetResult();
            }
            return conntection;
        }
        protected SqliteCommand GetCommand(SqliteConnection conn)
        {
            SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandTimeout = _timeout.HasValue ? _timeout.Value : 10;
            cmd.CommandType = CommandType.Text;
            return cmd;
        }
        public virtual void Clear(string sqlitename)
        {
            _intransaction = false;//TO DO: validate this
            if (_mutitonconnection.ContainsKey(sqlitename))
            {
                var t = _mutitonconnection[sqlitename];
                if (t != null)
                {
                    t.Close();
                    t.Dispose();
                }
                _mutitonconnection.Remove(sqlitename);
            }
            if (_multitonconnectionwithtransaction.ContainsKey(sqlitename))
            {
                var t = _multitonconnectionwithtransaction[sqlitename];
                if (t.Connection != null)
                {
                    t.Connection.Close();
                    t.Connection.Dispose();
                }
                if (t.Transaction != null)
                {
                    t.Transaction.Dispose();
                }
                _multitonconnectionwithtransaction.Remove(sqlitename);
            }
        }
        public virtual void Clear()
        {
            //_intransaction = false;
            _mutitonconnection.ToList().ForEach(t =>
            {
                Clear(t.Key);
                //if (t.Value != null)
                //{
                //    t.Value.Close();
                //    t.Value.Dispose();
                //}
            });
            _multitonconnectionwithtransaction.ToList().ForEach(t =>
            {
                Clear(t.Key);
                //if (t.Value.Connection != null)
                //{
                //    t.Value.Connection.Close();
                //    t.Value.Connection.Dispose();
                //}
                //if (t.Value.Transaction != null)
                //{
                //    t.Value.Transaction.Dispose();
                //}
            });
            //_mutitonconnection.Clear();
            //_multitonconnectionwithtransaction.Clear();
        }
        public virtual void Commit()
        {
            if (_intransaction && _withtransaction)
            {
                _multitonconnectionwithtransaction[SqliteFileName].Transaction.Commit();
                _multitonconnectionwithtransaction.Remove(SqliteFileName);
                _withtransaction = false;
                _intransaction = false;
            }
            else
            {
                //Console.WriteLine("Should not be called.. ");//TO DO: throw error
            }
        }
        public virtual void Rollback()
        {
            if (_intransaction && _withtransaction)
            {
                _multitonconnectionwithtransaction[SqliteFileName].Transaction.Rollback();
                _multitonconnectionwithtransaction.Remove(SqliteFileName);
                _intransaction = _withtransaction = false;
            }
            else
            {
                //Console.WriteLine("Should not be called.. ");//TO DO: throw error
            }
        }
        protected List<T> GetList<T>(string query, DbParam[] param)
        {
            return SerializeDatasetToList<T>(GetDataSet(query, param)) ?? new List<T>();
        }
        protected List<T> GetList<T>(string query)
        {
            return GetList<T>(query, null);
        }
        protected async Task<List<T>> GetListAsync<T>(string query)
        {
            return await Task.FromResult(GetList<T>(query));
        }
        protected T GetItem<T>(string query)
        {
            return GetItem<T>(query, null);
        }
        protected async Task<T> GetItemAsync<T>(string query)
        {
            return await Task.FromResult(GetItem<T>(query, null));
        }
        protected async Task<T> GetItemAsync<T>(string query, DbParam[] param)
        {
            return await Task.FromResult(GetItem<T>(query, param));
        }
        protected T GetItem<T>(string query, DbParam[] param)
        {
            var rest = GetList<T>(query, param);
            if (rest == null || rest.Count == 0)
                return default(T);
            return rest[0];
        }
        protected DataSet GetDataSet(string query)
        {
            return GetDataSet(query, null);
        }
        protected async Task<DataSet> GetDataSetAsync(string query)
        {
            return await Task.FromResult(GetDataSet(query));
        }
        protected async Task<DataSet> GetDataSetAsync(string query, DbParam[] param)
        {
            return await Task.FromResult(GetDataSet(query, param));
        }
        protected DataSet GetDataSet(string query, DbParam[] param)
        {
            lock (_lock)
            {
                var con = GetConnection();
                //using (SqliteConnection con = GetConnection())
                //{
                try
                {
                    if (con != null && con.State == ConnectionState.Open)
                    {
                        using (var cmd = GetCommand(con))
                        {
                            cmd.CommandText = query;
                            if (param != null) AssignParameters(cmd, param);
                            using (Microsoft.Data.Sqlite.SqliteDa SqliteDataAdapter adapter = new SqliteDataAdapter())
                            {
                                adapter.SelectCommand = cmd;
                                DataSet ds = new DataSet();
                                adapter.Fill(ds);
                                return ds;
                            }
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
                //}/data/user/0/com.winit.msfa.droid.app/files/SalesmanDb.sqlite
            }
        }
        protected int Insert(string query, DbParam[] param)
        {
            lock (_lock)
            {
                var con = GetConnection();
                //using (SqliteConnection con = GetConnection())
                //{
                if (con != null && con.State == ConnectionState.Open)
                {
                    using (SqliteCommand cmd = GetCommand(con))
                    {
                        cmd.CommandText = query;
                        cmd.CommandTimeout = 20000;
                        if (param != null) AssignParameters(cmd, param);
                        try { return (int)cmd.ExecuteNonQuery(); }
                        catch (Exception err)
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    return 0;
                }
            }
        }
        protected int Insert(string query)
        {
            return Insert(query, null);
        }
        protected T GetScalar<T>(string query)
        {
            return GetScalar<T>(query, null);
        }
        protected T GetScalar<T>(string query, DbParam[] param)
        {
            lock (_lock)
            {
                var con = GetConnection();
                //using (SqliteConnection con = GetConnection())
                //{
                if (con != null && con.State == ConnectionState.Open)
                {
                    using (SqliteCommand cmd = GetCommand(con))
                    {
                        cmd.CommandText = query;
                        if (param != null) AssignParameters(cmd, param);
                        var res = cmd.ExecuteScalar();
                        return (T)(res ?? default(T));
                    }
                }
                else
                {
                    return default(T);
                }
                //}
            }
        }
        protected T GetValue<T>(string query, DbParam[] param)
        {
            var rest = GetList<T>(query, param);
            if (rest == null || rest.Count == 0)
                return default(T);
            return rest[0];
        }
        protected T GetValue<T>(string query)
        {
            return GetValue<T>(query, null);
        }
        private SqliteCommand AssignParameters(SqliteCommand command, DbParam[] spParams)
        {
            if (spParams != null)
            {
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                foreach (DbParam spParam in spParams)
                {
                    SqliteParameter spParameter = new SqliteParameter();

                    spParameter.DbType = spParam.ParamType;
                    spParameter.ParameterName = spParam.ParamName;
                    spParameter.Value = spParam.ParamValue;
                    spParameter.Direction = spParam.ParamDirection;
                    spParameter.SourceColumn = spParam.ParamSourceColumn;
                    spParameter.Size = spParam.Size;
                    command.Parameters.Add(spParameter);
                }
                sw.Stop();
                //Console.WriteLine($"[Params Duration] : {DateTime.Now.ToString("dd'/'MM'/'yyyy HHmmss.ffffff")}--{sw.ElapsedMilliseconds}");
            }
            return command;
        }
    }

    public static class SerExtn
    {
        private static IEnumerable<Dictionary<string, object>> Serialize(this SqliteDataReader reader)
        {
            var results = new List<Dictionary<string, object>>();
            var cols = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++)
            { cols.Add(reader.GetName(i)); }

            while (reader.Read())
            {
                results.Add(SerializeRow(cols, reader));
            }

            reader.Close();

            return results;
        }
        private static Dictionary<string, object> SerializeRow(IEnumerable<string> cols,
                                                       SqliteDataReader reader)
        {
            var result = new Dictionary<string, object>();
            foreach (var col in cols)
            {
                if (reader[col] != DBNull.Value)
                {
                    result.Add(col, reader[col]);
                }
            }

            return result;
        }
        private static Dictionary<string, object> SerializeSingle(this SqliteDataReader reader)
        {
            var result = new Dictionary<string, object>();
            var cols = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++)
            { cols.Add(reader.GetName(i)); }

            if (reader.Read())
            {
                result = SerializeRow(cols, reader);
            }

            reader.Close();

            return result;
        }
        public static dynamic SerializeToList<T>(this SqliteDataReader reader)
        {
            var r = Serialize(reader);
            string serilizedInString = JsonConvert.SerializeObject(r);
            return JsonConvert.DeserializeObject<T>(serilizedInString);
        }

        public static dynamic SerializeToObject<T>(this SqliteDataReader reader)
        {
            var r = SerializeSingle(reader);
            string serilizedInString = JsonConvert.SerializeObject(r);
            return JsonConvert.DeserializeObject<T>(serilizedInString);
        }
    }
}