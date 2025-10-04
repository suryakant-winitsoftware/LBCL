using SyncManagerModel.Base;
using SyncManagerModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerModel.Classes
{
    public class TaxMaster :SyncBaseModel, ITaxMaster
    {
        public long SyncLogId { get; set; }
        public string? UID { get; set; }
        public string HsnCode { get; set; }
        public string TaxPercentage { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
