using Winit.Modules.Invoice.Model.Interfaces;

namespace Winit.Modules.Invoice.Model.Classes;

public class InvoiceMaster:IInvoiceMaster
{
    public IInvoiceHeaderView Invoiceheader { get; set; } = new InvoiceHeaderView();
    public List<IInvoiceLineView> InvoiceLines { get; set; } = [];
}
