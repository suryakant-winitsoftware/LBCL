using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
{
    public class StoreCustomer : IStoreCustomer
    {
        public required string UID { get; set; }
        public required string Code { get; set; }
        public required string Label { get; set; }
        public required string Address { get; set; }
        public required string RouteCustomerUID { get; set; }
        public bool IsDeleted { get; set; }
        public int SeqNo { get; set; }
        public bool IsSelected { get; set; }
    }
}
