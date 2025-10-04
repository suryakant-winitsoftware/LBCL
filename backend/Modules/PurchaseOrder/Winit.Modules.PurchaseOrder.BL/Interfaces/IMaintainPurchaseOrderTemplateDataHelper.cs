using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.BL.Interfaces;

public interface IMaintainPurchaseOrderTemplateDataHelper
{
    Task<PagedResponse<IPurchaseOrderTemplateHeader>?> GetAllPurchaseOrderTemplateHeader(int pageNumber, int pageSize,
        List<SortCriteria> sortCriterias, List<FilterCriteria> filterCriterias, bool isCountRequired);
    Task<bool> DeletePurchaseOrderHeaderByUIDs(List<string> purchaseOrderTemplateHeaderUids);
}
