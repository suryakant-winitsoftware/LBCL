using System.Data;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.BL.Interfaces;

public interface IPurchaseOrderTemplateLineBL
{
    Task<PagedResponse<IPurchaseOrderTemplateLine>> GetAllPurchaseOrderTemplateLines(List<SortCriteria>? sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria>? filterCriterias, bool isCountRequired);
    Task<int> CreatePurchaseOrderTemplateLines(List<IPurchaseOrderTemplateLine> purchaseOrderTemplateLines);
    Task<int> UpdatePurchaseOrderTemplateLines(
        List<IPurchaseOrderTemplateLine> purchaseOrderTemplateLines);
    Task<int> DeletePurchaseOrderTemplateLinesByUIDs(List<string> purchaseOrderTemplateLineUids);
}
