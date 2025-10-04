using SyncManagerModel.Base;

namespace SyncManagerModel.Interfaces
{
    public interface ICustomer_Ref : ISyncBaseModel
    {
        public long SyncLogId { get; set; }
        public string UID { get; set; }
        public string RecUid { get; set; }
        public string CustomerCode { get; set; }
        public string IsActive { get; set; }
        public string IsBlocked { get; set; }
        public string BlockedReason { get; set; }
        public string CustomField1 { get; set; }
        public string CustomField2 { get; set; }
        public string CustomField3 { get; set; }
        public string CustomField4 { get; set; }
        public string CustomField5 { get; set; }

    }
}
