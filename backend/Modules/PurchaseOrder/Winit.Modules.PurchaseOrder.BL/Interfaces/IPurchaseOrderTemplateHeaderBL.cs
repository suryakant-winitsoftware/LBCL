using System.Data;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.BL.Interfaces;

public interface IPurchaseOrderTemplateHeaderBL
{
    Task<PagedResponse<IPurchaseOrderTemplateHeader>> GetAllPurchaseOrderTemplateHeaders(List<SortCriteria>? sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria>? filterCriterias, bool isCountRequired);
    Task<int> CreatePurchaseOrderTemplateHeaders(List<IPurchaseOrderTemplateHeader> purchaseOrderTemplateHeaders,
        IDbConnection? dbConnection = null, IDbTransaction? dbTransaction = null);
    Task<int> UpdatePurchaseOrderTemplateHeader(
        List<IPurchaseOrderTemplateHeader> purchaseOrderTemplateHeaders, IDbConnection? dbConnection = null, IDbTransaction? dbTransaction = null);
    Task<bool> CUD_PurchaseOrderTemplate(IPurchaseOrderTemplateMaster purchaseOrderTemplateMaster);
    Task<IPurchaseOrderTemplateMaster> GetPurchaseOrderTemplateMasterByUID(string uid);
    Task<int> DeletePurchaseOrderHeaderByUID(List<string> purchaseOrderHeaderUids);
    Task<List<IPurchaseOrderTemplateHeader>> GetPurchaseOrderTemplateHeadersByStoreUidAndCreatedby(string storeUid, string? createdBy = null);
}
