using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.PurchaseOrder.BL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.BL.Classes;

public class PurchaseOrderHeaderBL : IPurchaseOrderHeaderBL
{
    protected readonly DL.Interfaces.IPurchaseOrderHeaderDL _purchaseOrderHeaderDL;

    public PurchaseOrderHeaderBL(DL.Interfaces.IPurchaseOrderHeaderDL returnOrderDL, IServiceProvider serviceProvider)
    {
        _purchaseOrderHeaderDL = returnOrderDL;
    }


    public async Task<PagedResponse<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderHeaderItem>>
        GetPurchaseOrderHeadersAsync
        (List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        return await _purchaseOrderHeaderDL.GetPurchaseOrderHeadersAsync(sortCriterias, pageNumber, pageSize,
        filterCriterias, isCountRequired);
    }

    public async Task<bool> CUD_PurchaseOrder(List<IPurchaseOrderMaster> purchaseOrderMasters)
    {
        return await _purchaseOrderHeaderDL.CUD_PurchaseOrder(purchaseOrderMasters);
    }

    public async Task<IPurchaseOrderMaster> GetPurchaseOrderMasterByUID(string uid)
    {
        return await _purchaseOrderHeaderDL.GetPurchaseOrderMasterByUID(uid);
    }

    public async Task<Dictionary<string, int>> GetPurchaseOrderSatatusCounts(List<FilterCriteria>? filterCriterias = null)
    {
        return await _purchaseOrderHeaderDL.GetPurchaseOrderSatatusCounts(filterCriterias);
    }

    public async Task<int> UpdatePurchaseOrderHeaderStatusAfterApproval(IPurchaseOrderHeader purchaseOrderHeader)
    {
        return await _purchaseOrderHeaderDL.UpdatePurchaseOrderHeaderStatusAfterApproval(purchaseOrderHeader);
    }

    public async Task<List<IPurchaseOrderLineQPS>> GetPurchaseOrderLineQPSs(string orgUid, string schemeUid,
        List<string> skuUids = null)
    {
        return await _purchaseOrderHeaderDL.GetPurchaseOrderLineQPSs(orgUid, schemeUid, skuUids);
    }

    public async Task<bool> CreateApproval(string purchaseOrderUid, ApprovalRequestItem approvalRequestItem)
    {
        return await _purchaseOrderHeaderDL.CreateApproval(purchaseOrderUid, approvalRequestItem);
    }
}
