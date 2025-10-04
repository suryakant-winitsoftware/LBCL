namespace Winit.Modules.Invoice.Model.Interfaces;

public interface IInvoiceLineView
{
    public string UID { get; set; }
    public string InvoiceNo { get; set; }
    public string ItemCode { get; set; }
    public string ItemName { get; set; }
    public decimal OrderedQty { get; set; }
    public decimal ShippedQty { get; set; }
    public decimal CancelledQty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalTax { get; set; }
    public List<string>? SerialNo { get; set; }
}
