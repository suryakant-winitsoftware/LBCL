using System.ComponentModel.DataAnnotations;

namespace WinIt.Models.Customers
{
    public static class OrderDataRepository
    {
        public static List<OrderData> OrderDataList { get; } = new List<OrderData>
        {
            new OrderData
            {
                OrderNumber = 1001,
                Salesman = "John Doe",
                CustomerName = "Acme Corp",
                OrderDate = DateTime.Now.AddDays(-5),
                DeliveryDate = DateTime.Now.AddDays(2),
                Amount = 1500.00
            },
            new OrderData
            {
                OrderNumber = 1002,
                Salesman = "Jane Smith",
                CustomerName = "XYZ Ltd",
                OrderDate = DateTime.Now.AddDays(-10),
                DeliveryDate = DateTime.Now.AddDays(5),
                Amount = 2200.00
            },
            // ... Add other OrderData objects ...
        };
    }
    public class OrderData
    {
        public int OrderNumber { get; set; }
        public string Salesman { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public double Amount { get; set; }
    }
    public class customer
    {

       
            public string CompanyUID { get; set; }
            public string Code { get; set; }
            public string Number { get; set; }
            public string Name { get; set; }
            public string AliasName { get; set; }
            public string LegalName { get; set; }
            public string Type { get; set; }
            public string BillToStoreUID { get; set; }
            public string ShipToStoreUID { get; set; }
            public string SoldToStoreUID { get; set; }
            public int Status { get; set; }
            public bool IsActive { get; set; }
            public string StoreClass { get; set; }
            public string StoreRating { get; set; }
            public bool IsBlocked { get; set; }
            public string BlockedReasonCode { get; set; }
            public string BlockedReasonDescription { get; set; }
            public string CreatedByEmpUID { get; set; }
            public string CreatedByJobPositionUID { get; set; }
            public string CountryUID { get; set; }
            public string RegionUID { get; set; }
            public string CityUID { get; set; }
            public string Source { get; set; }
            public int Id { get; set; }
            public string UID { get; set; }
            public int SS { get; set; }
            public string CreatedBy { get; set; }
            public DateTime CreatedTime { get; set; }
            public string ModifiedBy { get; set; }
            public DateTime ModifiedTime { get; set; }
            public DateTime ServerAddTime { get; set; }
            public DateTime ServerModifiedTime { get; set; }
            public string ErrorMessage { get; set; }
            public bool IsValid { get; set; }
        



        //public string? CompanyUID { get; set; }
        //public string? Code { get; set; }
        //public string? Number { get; set; }
        //public string? Name { get; set; }
        //public string? AliasName { get; set; }
        //public string? Type { get; set; }
        //public string? BillToStoreUID { get; set; }
        //public string? ShipToStoreUID { get; set; }
        //public string? SoldToStoreUID { get; set; }
        //public int Status { get; set; }
        //public bool IsActive { get; set; }
        //public string? StoreClass { get; set; }
        //public string? StoreRating { get; set; }
        //public bool IsBlocked { get; set; }
        //public string? BlockedReasonCode { get; set; }
        //public string? BlockedReasonDescription { get; set; }
        //public string? CreatedByEmpUID { get; set; }
        //public string? CreatedByJobPositionUID { get; set; }
        //public string? CountryUID { get; set; }
        //public string? RegionUID { get; set; }
        //public string? CityUID { get; set; }
        //public string? Source { get; set; }
        //public object? StoreCredit { get; set; }
        //public string? VK { get; set; }
        //public object? VK2 { get; set; }
        //public int Id { get; set; }
        //public string? UID { get; set; }
        //public int SS { get; set; }
        //public string? CreatedBy { get; set; }
        //public DateTime CreatedTime { get; set; }
        //public string? ModifiedBy { get; set; }
        //public DateTime ModifiedTime { get; set; }
        //public DateTime ServerAddTime { get; set; }
        //public DateTime ServerModifiedTime { get; set; }
    }
}
