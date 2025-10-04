namespace Winit.Modules.AuditTrail.Model.Classes
{
    public class AuditDataNew
    {
        public SalesOrder SalesOrder { get; set; }
        public List<SalesOrderLine> SalesOrderLines { get; set; }
    }
    public class SalesOrder
    {
        public string SalesOrderNumber { get; set; }
        public string StoreUID { get; set; }
        public string OrderType { get; set; }
    }

    public class SalesOrderLine
    {
        public string SalesOrderUID { get; set; }
        public string SalesOrderLineUID { get; set; }
        public int LineNumber { get; set; }
        public string ItemCode { get; set; }
    }
}
