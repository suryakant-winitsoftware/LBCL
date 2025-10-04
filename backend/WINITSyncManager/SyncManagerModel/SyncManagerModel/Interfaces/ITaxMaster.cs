using SyncManagerModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerModel.Interfaces
{
    public interface ITaxMaster :ISyncBaseModel
    {
        public long SyncLogId { get; set; }
        public string? UID { get; set; }
        public string HsnCode { get; set; }
        public string TaxPercentage { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
