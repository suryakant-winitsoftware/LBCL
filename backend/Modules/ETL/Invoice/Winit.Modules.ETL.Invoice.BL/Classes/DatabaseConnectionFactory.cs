using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Npgsql;
using Microsoft.Extensions.Options;
using Winit.Modules.ETL.Invoice.BL.Configuration;

namespace Winit.Modules.ETL.Invoice.BL.Classes
{
    public interface IDatabaseConnectionFactory
    {
        IDbConnection CreateSourceConnection();
        IDbConnection CreateDestinationConnection();
    }

    public class DatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly DatabaseSettings _settings;

        public DatabaseConnectionFactory(IOptions<DatabaseSettings> settings)
        {
            _settings = settings.Value;
        }

        public IDbConnection CreateSourceConnection()
        {
            return CreateConnection(_settings.SourceConnectionString, _settings.SourceDbType);
        }

        public IDbConnection CreateDestinationConnection()
        {
            return CreateConnection(_settings.DestinationConnectionString, _settings.DestinationDbType);
        }

        private IDbConnection CreateConnection(string connectionString, string dbType)
        {
            return dbType.ToUpper() switch
            {
                "MSSQL" => new SqlConnection(connectionString),
                "PGSQL" => new NpgsqlConnection(connectionString),
                _ => throw new ArgumentException($"Unsupported database type: {dbType}")
            };
        }
    }
} 