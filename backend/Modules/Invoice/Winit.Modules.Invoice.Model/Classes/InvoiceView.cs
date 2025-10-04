using Winit.Modules.Invoice.Model.Interfaces;
namespace Winit.Modules.Invoice.Model.Classes;

public class InvoiceView : IInvoiceView
{
    public string UID { get; set; }
    public string InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal AvailableQty { get; set; }
}
