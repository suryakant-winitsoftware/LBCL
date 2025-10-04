using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model.Enum;

namespace Winit.Modules.Base.DL.DBManager
{
    public abstract class DBManagerBaseV1
    {
        protected readonly ILogger<DBManagerBaseV1> Logger;
        protected readonly DBType DBType;

        protected DBManagerBaseV1(ILogger<DBManagerBaseV1> logger, DBType dbType)
        {
            Logger = logger;
            DBType = dbType;
        }

        protected IDbConnection CreateConnection(string connectionString)
        {
            // Use the DBType enum to determine the database type
            switch (DBType)
            {
                case DBType.MSSQL:
                    return new SqlConnection(connectionString);
                case DBType.PGSQL:
                    return new NpgsqlConnection(connectionString);
                default:
                    throw new ArgumentException($"Unsupported database type: {DBType}");
            }
        }
    }
}
