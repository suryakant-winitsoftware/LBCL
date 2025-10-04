namespace Winit.Modules.ReturnOrder.Model.Interfaces;

public interface IReturnOrderLineInvoice
{
    public string SKUCode { get; set; }
    public string SKUName { get; set; }
    public string SKUImage { get; set; }
    public string UOM { get; set; }
    public decimal OrderQty { get; set; }
    public string ItemType { get; set; }
    public string Reason { get; set; }
}
