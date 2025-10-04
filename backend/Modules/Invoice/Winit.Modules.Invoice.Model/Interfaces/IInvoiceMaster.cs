namespace Winit.Modules.Invoice.Model.Interfaces;

public interface IInvoiceMaster
{
    public IInvoiceHeaderView Invoiceheader { get; set; }
    public List<IInvoiceLineView> InvoiceLines { get; set; }
}
