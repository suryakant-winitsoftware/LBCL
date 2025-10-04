using SyncManagerModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerModel.Interfaces
{
    public interface IStatementAndBalanceConfi :ISyncBaseModel
    {
        public long SyncLogId { get; set; }
        public string UID { get; set; }
        public string SalesOffice { get; set; }
        public string Branch { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerLocationCode { get; set; }
        public string OperatingUnit { get; set; }
        public string TransactionType { get; set; }
        public string TrxNumber { get; set; }
        public string DocumentNumber { get; set; }
        public string Description { get; set; }
        public DateTime TrxDate { get; set; }
        public decimal DrAmount { get; set; }
        public decimal CrAmount { get; set; }
    }
}
