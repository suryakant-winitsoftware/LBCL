namespace Winit.Modules.SalesOrder.Model.Interfaces
{
    public interface ISalesOrderViewModel
    {
        public ISalesOrder SalesOrder { get; set; }
        public List<ISalesOrderLine> SalesOrderLine { get; set; }
    }
}
