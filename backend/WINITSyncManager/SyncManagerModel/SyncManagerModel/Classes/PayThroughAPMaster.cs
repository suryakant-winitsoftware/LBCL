using SyncManagerModel.Base;
using SyncManagerModel.Interfaces;

namespace SyncManagerModel.Classes
{
    public class PayThroughAPMaster : SyncBaseModel, IPayThroughAPMaster
    {
        public long SyncLogId { get; set; }
        public string? UID { get; set; }
        public string Division { get; set; }
        public string BrandCategory { get; set; }
        public string SkuCode { get; set; }
        public decimal ConsumerFinance { get; set; }
        public decimal ServiceCommission { get; set; }
        public string FreeInstallation { get; set; }
        public string FTS { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CustomField1 { get; set; }
        public string CustomField2 { get; set; }
        public string CustomField3 { get; set; }
        public string CustomField4 { get; set; }
        public string CustomField5 { get; set; }
        public string S_RW_ID { get; set; }
    }
}
