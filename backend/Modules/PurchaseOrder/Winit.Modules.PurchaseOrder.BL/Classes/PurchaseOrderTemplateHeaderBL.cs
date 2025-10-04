using System.Data;
using Winit.Modules.PurchaseOrder.BL.Interfaces;
using Winit.Modules.PurchaseOrder.DL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.BL.Classes;

public class PurchaseOrderTemplateHeaderBL : IPurchaseOrderTemplateHeaderBL
{
    private readonly IPurchaseOrderTemplateHeaderDL _purchaseOrderTemplateHeaderDL;

    public PurchaseOrderTemplateHeaderBL(IPurchaseOrderTemplateHeaderDL purchaseOrderTemplateHeaderDL)
    {
        _purchaseOrderTemplateHeaderDL = purchaseOrderTemplateHeaderDL;
    }

    public async Task<int> CreatePurchaseOrderTemplateHeaders(List<IPurchaseOrderTemplateHeader> purchaseOrderTemplateHeaders,
        IDbConnection? dbConnection = null, IDbTransaction? dbTransaction = null)
    {
        return await _purchaseOrderTemplateHeaderDL
            .CreatePurchaseOrderTemplateHeaders(purchaseOrderTemplateHeaders, dbConnection, dbTransaction);
    }

    public async Task<bool> CUD_PurchaseOrderTemplate(IPurchaseOrderTemplateMaster purchaseOrderTemplateMaster)
    {
        return await _purchaseOrderTemplateHeaderDL.CUD_PurchaseOrderTemplate(purchaseOrderTemplateMaster);
    }


    public async Task<PagedResponse<IPurchaseOrderTemplateHeader>> GetAllPurchaseOrderTemplateHeaders
        (List<SortCriteria>? sortCriterias, int pageNumber, int pageSize, List<FilterCriteria>? filterCriterias,
        bool isCountRequired)
    {
        return await _purchaseOrderTemplateHeaderDL
            .GetAllPurchaseOrderTemplateHeaders(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
    }

    public async Task<IPurchaseOrderTemplateMaster> GetPurchaseOrderTemplateMasterByUID(string uid)
    {
        return await _purchaseOrderTemplateHeaderDL.GetPurchaseOrderTemplateMasterByUID(uid);
    }

    public async Task<int> UpdatePurchaseOrderTemplateHeader(List<IPurchaseOrderTemplateHeader> purchaseOrderTemplateHeaders,
        IDbConnection? dbConnection = null, IDbTransaction? dbTransaction = null)
    {
        return await _purchaseOrderTemplateHeaderDL
            .UpdatePurchaseOrderTemplateHeader(purchaseOrderTemplateHeaders, dbConnection, dbTransaction);
    }
    public async Task<int> DeletePurchaseOrderHeaderByUID(List<string> purchaseOrderHeaderUids)
    {
        return await _purchaseOrderTemplateHeaderDL.DeletePurchaseOrderHeaderByUID(purchaseOrderHeaderUids);
    }

    public async Task<List<IPurchaseOrderTemplateHeader>> GetPurchaseOrderTemplateHeadersByStoreUidAndCreatedby(string storeUid, string? createdBy = null)
    {
        return await _purchaseOrderTemplateHeaderDL.GetPurchaseOrderTemplateHeadersByStoreUidAndCreatedby(storeUid, createdBy);
    }
}
