namespace Winit.Modules.Invoice.Model.Interfaces;

public interface IInvoiceView
{
    string UID { get; set; }
    string InvoiceNumber { get; set; }
    DateTime InvoiceDate { get; set; }
    decimal AvailableQty { get; set; }
}
