using Winit.Modules.Invoice.Model.Interfaces;

namespace Winit.Modules.Invoice.Model.Classes;

public class InvoiceLineView:IInvoiceLineView
{
    public string UID { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal OrderedQty { get; set; }
    public decimal ShippedQty { get; set; }
    public decimal CancelledQty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalTax { get; set; }
    public List<string>? SerialNo { get; set; }
}
