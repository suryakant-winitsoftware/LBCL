using System;

namespace Winit.Modules.ETL.Invoice.BL.Configuration
{
    public class DatabaseSettings
    {
        public string SourceConnectionString { get; set; }
        public string DestinationConnectionString { get; set; }
        public string SourceDbType { get; set; }  // "MSSQL" or "PGSQL"
        public string DestinationDbType { get; set; }  // "MSSQL" or "PGSQL"
    }
} 