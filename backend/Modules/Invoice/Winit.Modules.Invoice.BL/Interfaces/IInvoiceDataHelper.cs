using Winit.Modules.Invoice.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Invoice.BL.Interfaces;

public interface IInvoiceDataHelper
{
    Task<PagedResponse<IInvoiceHeaderView>?> GetAllInvoices(List<SortCriteria>? sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria>? filterCriterias, bool isCountRequired);
    Task<IInvoiceMaster?> GetInvoiceMasterByInvoiceUID(string invoiceUID);
    Task<List<IStore>?> GetChannelPartner(string jobPositionUid);
}
