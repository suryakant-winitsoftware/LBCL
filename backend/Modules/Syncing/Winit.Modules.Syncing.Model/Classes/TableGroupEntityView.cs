using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Syncing.Model.Classes
{
    public class TableGroupEntityView: Interfaces.ITableGroupEntityView
    {
        /// <summary>
        /// Represents the name of the group associated with the table.
        /// </summary>
        public string GroupName { get; set; }
        /// <summary>
        /// Represents the name of the table to be synced.
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// Represents the serial number associated with the table.
        /// </summary>
        public int SerialNo { get; set; }
        /// <summary>
        /// Represents the query for master data
        /// </summary>
        public string MasterDataQuery { get; set; }
        /// <summary>
        /// Represents the query for sync data
        /// </summary>
        public string SyncDataQuery { get; set; }
        /// <summary>
        /// Represents the query for Sqlite Insert
        /// </summary>
        public string SqliteInsertQuery { get; set; }
        /// <summary>
        /// Represents the comma separated parameter for Sqlite Insert
        /// </summary>
        public string SqliteInsertParameter { get; set; }
        /// <summary>
        /// Represents the Model Name which will be mapped to MasterDataQuery & SyncDataQuery
        /// </summary>
        public string ModelName { get; set; }
        /// <summary>
        /// Represents the query for Sqlite Update
        /// </summary>
        public string SqliteUpdateQuery { get; set; }
        /// <summary>
        /// Represents the last downloaded time for the entity
        /// </summary>
        public DateTime LastDownloadedTime { get; set; }
    }
}
