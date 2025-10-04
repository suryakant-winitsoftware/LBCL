using SyncManagerModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerModel.Classes
{
    public class IntegrationProcessStatus: IIntegrationProcessStatus
    {
        public long? ProcessId { get; set; }
        public string? InterfaceName { get; set; }
        public string? MonthTableName { get; set; }
        public string? TablePrefix { get; set; }
        public long SyncLogId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ProcessedOn { get; set; }  
        public int ProcessStatus { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ReqBatchNumber { get; set; }
        public int? OracleStatus { get; set; }
    }
}
