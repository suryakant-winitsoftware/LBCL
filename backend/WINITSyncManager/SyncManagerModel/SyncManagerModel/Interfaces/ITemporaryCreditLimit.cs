using SyncManagerModel.Base;

namespace SyncManagerModel.Interfaces
{
    public interface ITemporaryCreditLimit : ISyncBaseModel
    {
        public long SyncLogId { get; set; }
        public string UID { get; set; }
        public string ReqUID { get; set; }
        public string RequestNumber { get; set; }
        public string CustomerCode { get; set; }
        public string RequestType { get; set; }
        public string Division { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal RequestedCreditLimit { get; set; }
        public decimal RequestedCreditDays { get; set; }
        public string Remarks { get; set; }
        public string OracleStatus { get; set; }
        public int ReadFromOracle { get; set; }
        public string IsAutoApproved { get; set; }
    }
}
