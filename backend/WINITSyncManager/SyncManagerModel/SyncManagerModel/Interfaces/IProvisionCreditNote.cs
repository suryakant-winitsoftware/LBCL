using SyncManagerModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerModel.Interfaces
{
    public interface IProvisionCreditNote:ISyncBaseModel
    {
        public long SyncLogId { get; set; }
        public string? UID { get; set; }
        public string? ProvisionId { get; set; }
        public string? DmsReleaseRequested { get; set; }
        public int OracleProcessed { get; set; }
        public string? CnNumber { get; set; }
        public DateTime? CnDate { get; set; }  
        public decimal CnAmount { get; set; }
    }
}
