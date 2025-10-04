using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SalesOrder.BL.UIInterfaces;

public interface ISalesOrderAppViewModel : ISalesOrderViewModel
{
    public Dictionary<ISelectionItem, List<ISelectionItem>> FilterDataList { get; set; }
    public string LeftAttributeCode { get; set; }
    public string TopAttributeCode { get; set; }
    public bool IsLeftParentOfTop { get; set; }
    public FilterCriteria LeftScrollFilterCriteria { get; set; }
    public List<FilterCriteria> TopScrollFilterCriterias { get; set; }
    void PopulateFilterData();
    List<SelectionItemFilter> GetTopScrollSelectionItems(string? leftScrollItemCode = null);
    List<SelectionItemFilter> GetLeftScrollSelectionItems();
    Task<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderPrintView> GetSalesOrderHeaderDetails();
    Task<List<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLinePrintView>> GetSalesOrderlines();
}
