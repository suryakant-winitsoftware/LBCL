using SyncManagerModel.Base;
using SyncManagerModel.Interfaces;

namespace SyncManagerModel.Classes
{
    public class IntegrationMessageProcess : SyncBaseModel, IIntegrationMessageProcess
    {
        public long ProcessId { get; set; }
        public string InterfaceName { get; set; }
        public string MonthTableName { get; set; }
        public string TablePrefix { get; set; }
        public long? SyncLogDetailId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ProcessedOn { get; set; }
        public int? ProcessStatus { get; set; }
        public string ErrorMessage { get; set; }
        public string ReqBatchNumber { get; set; }
    }
}
