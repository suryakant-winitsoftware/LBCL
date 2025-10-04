using System.Data;
using Winit.Modules.PurchaseOrder.BL.Interfaces;
using Winit.Modules.PurchaseOrder.DL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.BL.Classes;

public class PurchaseOrderTemplateLineBL : IPurchaseOrderTemplateLineBL
{
    private readonly IPurchaseOrderTemplateLineDL _purchaseOrderTemplateLineDL;

    public PurchaseOrderTemplateLineBL(IPurchaseOrderTemplateLineDL purchaseOrderTemplateLineDL)
    {
        _purchaseOrderTemplateLineDL = purchaseOrderTemplateLineDL;
    }

    public async Task<int> CreatePurchaseOrderTemplateLines(List<IPurchaseOrderTemplateLine> purchaseOrderTemplateLines)
    {
        return await _purchaseOrderTemplateLineDL.CreatePurchaseOrderTemplateLines(purchaseOrderTemplateLines);
    }

    public async Task<PagedResponse<IPurchaseOrderTemplateLine>> GetAllPurchaseOrderTemplateLines(List<SortCriteria>? sortCriterias, int pageNumber, int pageSize, List<FilterCriteria>? filterCriterias, bool isCountRequired)
    {
        return await _purchaseOrderTemplateLineDL.GetAllPurchaseOrderTemplateLines(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
    }

    public async Task<int> UpdatePurchaseOrderTemplateLines(List<IPurchaseOrderTemplateLine> purchaseOrderTemplateLines)
    {
        return await _purchaseOrderTemplateLineDL.UpdatePurchaseOrderTemplateLines(purchaseOrderTemplateLines);
    }

    public async Task<int> DeletePurchaseOrderTemplateLinesByUIDs(List<string> purchaseOrderTemplateLineUids)
    {
        return await _purchaseOrderTemplateLineDL.DeletePurchaseOrderTemplateLinesByUIDs(purchaseOrderTemplateLineUids);
    }
}
