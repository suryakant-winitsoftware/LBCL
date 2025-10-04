using SyncManagerModel.Base;

namespace SyncManagerModel.Interfaces
{
    public interface ICustomerMasterDetails : ISyncBaseModel
    {
        public long SyncLogDetailId { get; set; }
        public string? UID { get; set; }
        public string? StoreUID { get; set; }
        public string? StoreCode { get; set; }
        public string? CustomerName { get; set; }
        public string? SalesOffice { get; set; }
        public string? Classification { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? Pincode { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? FirstName { get; set; }
        public string? PhnNo { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? PanNoGstNo { get; set; }
        public string? Warehouse { get; set; }
        public string? Purpose { get; set; }
        public string? PrimaryCustomer { get; set; }
        public string? LegalName { get; set; }
        public string? AddressKey { get; set; }
        public string? OracleCustomerCode { get; set; }
        public string? OracleLocationCode { get; set; }
        public string? Site_Number { get; set; }
        public int? ReadFromOracle { get; set; }
    }
}
