using Winit.Modules.Invoice.Model.Classes;
using Winit.Modules.Invoice.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
namespace Winit.Modules.ReturnOrder.BL.Interfaces;

public interface IPOReturnOrderDataHelper
{
    Task<List<IStore>?> GetChannelPartner(string jobPositionUid);
    Task<IStoreMaster?> GetStoreMasterByStoreUID(string storeUID);
    Task<List<IInvoiceView>> GetInvoicesForReturnOrder(InvoiceListRequest invoiceListRequest);
}
