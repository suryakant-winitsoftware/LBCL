using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;
using System.Reflection;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Common;
using Oracle.ManagedDataAccess.Client;

namespace SyncManagerDL.Base.DBManager
{
    public class DBManagerBase
    {
        public readonly IServiceProvider _serviceProvider;
        protected readonly string _connectionString = string.Empty;
        private OracleConnection _connection;
        private OracleCommand _cmd;
        private IAppConfig _appConfig { get; set; }
        public DBManagerBase(IServiceProvider serviceProvider, IConfiguration config, string connectionStringName)
        {
            _serviceProvider = serviceProvider;
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

        protected SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
        //protected OracleConnection OracleConnection()
        //{
        //    return new OracleConnection(_connectionString);
        //}

        public OracleConnection GetOracleConnection()
        {
            _connection = new OracleConnection(_connectionString);
            return _connection;
        }

        public OracleCommand GetOracleCommand()
        {
            _cmd = _connection.CreateCommand();
            return _cmd;
        }
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
            string appBaseDirectory = _appConfig.BaseFolderPath;//Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //string folderPath = Path.Combine(appBaseDirectory, @"Data\DB");
            string fileName = "WINITSQLite.db";
            //string fileName = "SalesForceAutomation.db";
            //string folderPath = @"D:\winit\WINITAPPLICATION1\SQLITE";
            return $"Data Source={Path.Combine(appBaseDirectory, fileName)};";
        }
        public string GetSqliteConnectionString(string sqliteFilePath)
        {
            return $"Data Source={sqliteFilePath};";
        }
    }
}

