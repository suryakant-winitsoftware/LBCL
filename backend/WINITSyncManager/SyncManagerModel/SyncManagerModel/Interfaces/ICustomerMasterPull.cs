using SyncManagerModel.Base;

namespace SyncManagerModel.Interfaces
{
    public interface ICustomerMasterPull : ISyncBaseModel
    {
        public long SyncLogId { get; set; }
        public string? UID { get; set; }
        public string? AddressKey { get; set; }
        public string? OracleCustomerCode { get; set; }
        public string? OracleLocationCode { get; set; }
        public string? Site_Number { get; set; }
        public int? ReadFromOracle { get; set; }

    }
}
