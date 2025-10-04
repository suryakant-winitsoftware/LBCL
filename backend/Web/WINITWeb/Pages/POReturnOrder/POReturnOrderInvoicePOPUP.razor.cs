using Microsoft.AspNetCore.Components;
using Winit.Modules.Invoice.Model.Classes;
using Winit.Modules.Invoice.Model.Interfaces;

namespace WinIt.Pages.POReturnOrder;

public partial class POReturnOrderInvoicePOPUP : ComponentBase
{
    [Parameter]
    public IEnumerable<IInvoiceView> Invoices { get; set; }
    [Parameter]
    public EventCallback<string> OnInvoiceSelect { get; set; }
    [Parameter]
    public EventCallback OnCloseClick { get; set; }
    [Parameter]
    public string SelectedInvoice { get; set; }
    [Parameter]
    public string InvoiceSearchString { get; set; }
}
