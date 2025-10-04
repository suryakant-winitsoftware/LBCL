using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.DL.Interfaces;

public interface IPurchaseOrderHeaderDL
{
    Task<PagedResponse<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderHeaderItem>>
        GetPurchaseOrderHeadersAsync(List<SortCriteria> sortCriterias, int pageNumber, int pageSize,
        List<FilterCriteria> filterCriterias, bool isCountRequired);

    Task<bool> CUD_PurchaseOrder(List<IPurchaseOrderMaster> purchaseOrderMasters);
    Task<Dictionary<string, int>> GetPurchaseOrderSatatusCounts(List<FilterCriteria>? filterCriterias = null);
    Task<IPurchaseOrderMaster> GetPurchaseOrderMasterByUID(string uid);
    Task<int> UpdatePurchaseOrderHeaderStatusAfterApproval(IPurchaseOrderHeader purchaseOrderHeader);
    Task<List<IPurchaseOrderLineQPS>> GetPurchaseOrderLineQPSs(string orgUid, string schemeUid, List<string> skuUids = null);
    Task<bool> CreateApproval(string purchaseOrderUid, ApprovalRequestItem approvalRequestItem);
}
