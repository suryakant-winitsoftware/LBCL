using SyncManagerModel.Base;
using SyncManagerModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerModel.Classes
{
    public class PricingMaster :SyncBaseModel,  IPricingMaster
    {
        public long SyncLogId { get; set; }
        public string? UID { get; set; }
        public string? name { get; set; }
        public int? status { get; set; }
        public string? message { get; set; }
        
        public int PriceMasterId { get; set; }
        public string ItemCode { get; set; }
        public decimal Mrp { get; set; }
        public decimal Dp { get; set; }
        public decimal MinSellingPrice { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }
}
