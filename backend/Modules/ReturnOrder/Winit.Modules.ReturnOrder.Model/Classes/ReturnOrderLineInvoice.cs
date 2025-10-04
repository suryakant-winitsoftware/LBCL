using Winit.Modules.ReturnOrder.Model.Interfaces;

namespace Winit.Modules.ReturnOrder.Model.Classes;

public class ReturnOrderLineInvoice : IReturnOrderLineInvoice
{
    public string SKUCode { get; set; } = string.Empty;
    public string SKUName { get; set; } = string.Empty;
    public string SKUImage { get; set; } = string.Empty;
    public string UOM { get; set; } = string.Empty;
    public decimal OrderQty { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
