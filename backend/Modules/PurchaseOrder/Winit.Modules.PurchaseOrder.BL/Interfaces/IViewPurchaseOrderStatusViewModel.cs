using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.BL.Interfaces;

public interface IViewPurchaseOrderStatusViewModel
{
    List<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderHeaderItem> DisplayHeaderList { get; set; }
    string Status { get; set; }
    List<ISelectionItem> OracleOrderStatusSelectionItems { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    List<ISelectionItem> ChannelPartnerSelectionItems { get; set; }
    List<FilterCriteria> FilterCriterias { get; set; }
    Task PopulateViewModel();
    Task<Dictionary<string, int>> GetTabItemsCount(List<FilterCriteria> filterCriterias);
    Task OnSorting(SortCriteria sortCriteria);
    Task PageIndexChanged(int pageNumber);
    Task ApplyFilter(List<FilterCriteria> filterCriterias);
    Task LoadChannelPartner();
}
