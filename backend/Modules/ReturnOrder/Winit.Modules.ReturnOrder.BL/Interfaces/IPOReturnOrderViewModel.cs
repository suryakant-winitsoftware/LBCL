using Winit.Modules.Invoice.Model.Classes;
using Winit.Modules.Invoice.Model.Interfaces;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
namespace Winit.Modules.ReturnOrder.BL.Interfaces;

public interface IPOReturnOrderViewModel
{
    public bool IsNewOrder { get; set; }
    public bool IsDraftOrder { get; set; }
    public IReturnOrder ReturnOrder { get; set; }
    public IStoreMaster? SelectedStoreMaster { get; set; }
    public string? SelectedDistributor { get; set; }
    public string? StoreSearchString { get; set; }
    public string? InvoiceSearchString { get; set; }
    public IEnumerable<IStore> FilteredStores { get; }
    public List<IPOReturnOrderLineItem> POReturnOrderLineItems { get; set; }
    public List<IPOReturnOrderLineItem> FilteredPOReturnOrderLineItems { get; set; }
    public InvoiceListRequest InvoiceListRequest { get; }
    public IEnumerable<IInvoiceView> FilteredInvoiceViews { get; }
    public IInvoiceMaster? SelectedInvoiceMaster { get; set; }
    public string SelectedInvoiceUID { get; set; }
    Task PopulateViewModel(string source, string orderUID = "");
    Task PrepareDistributors();
    Task OnDistributorSelect();

}
