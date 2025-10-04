using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.BL.Interfaces;

public interface IMaintainPurchaseOrderTemplateViewModel
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public string Status { get; set; }
    public List<IPurchaseOrderTemplateHeader> PurchaseOrderTemplateHeaders { get; set; }
    Task PopulateViewModel();
    Task PageIndexChanged(int index);
    Task OnSortClick(SortCriteria sortCriteria);
    Task OnFilterApply(Dictionary<string, string> keyValuePairs);
    Task OnDeleteClick(List<string> purchaseOrderTemplateHeaderUids);
}
