using System.Data;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.DL.Interfaces;

public interface IPurchaseOrderTemplateLineDL
{
    Task<PagedResponse<IPurchaseOrderTemplateLine>> GetAllPurchaseOrderTemplateLines(List<SortCriteria>? sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria>? filterCriterias, bool isCountRequired);
    Task<int> CreatePurchaseOrderTemplateLines(List<IPurchaseOrderTemplateLine> purchaseOrderTemplateLines,
      IDbConnection? dbConnection = null, IDbTransaction? dbTransaction = null);
    Task<int> UpdatePurchaseOrderTemplateLines(
        List<IPurchaseOrderTemplateLine> purchaseOrderTemplateLines, IDbConnection? dbConnection = null,
        IDbTransaction? dbTransaction = null);
    Task<int> DeletePurchaseOrderTemplateLinesByUIDs(List<string> purchaseOrderTemplateLineUids,
        IDbConnection? dbConnection = null, IDbTransaction? dbTransaction = null);
    Task<int> DeletePurchaseOrderTemplateLinesByPurchaseOrderTemplateHeaderUIDs(List<string> purchaseOrderTemplateHeaderUids,
        IDbConnection? dbConnection = null, IDbTransaction? dbTransaction = null);
}
