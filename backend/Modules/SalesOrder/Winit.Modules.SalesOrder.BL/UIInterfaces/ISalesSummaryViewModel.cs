using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SalesOrder.BL.UIInterfaces;

public interface ISalesSummaryViewModel
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<ISelectionItem> TabList { get; set; }
    public ISelectionItem SelectedTab { get; set; }
    public Dictionary<string, List<Model.UIInterfaces.ISalesSummaryItemView>> SalesSummaryItemViewsDictionary { get; set; }
    public List<Model.UIInterfaces.ISalesSummaryItemView>? DisplayedSalesSummaryItemViews { get; set; }
    /// <summary>
    /// Populate Initial Data
    /// </summary>
    void PopulateViewModel(Winit.Modules.SalesOrder.BL.Interfaces.ISalesOrderBL salesOrderBL)
    {
        PopulateSalesSummaryItemViewsDictionary(salesOrderBL);
    }
    Task PopulateSalesSummaryItemViewsDictionary(Winit.Modules.SalesOrder.BL.Interfaces.ISalesOrderBL salesOrderBL);
    /// <summary>
    /// Dispose object Reference
    /// </summary>
    void Dispose();
    /// <summary>
    /// Apply Search
    /// </summary>
    /// <param name="searchString"></param>
    /// <returns></returns>
    Task ApplySearch(string searchString);
    /// <summary>
    /// Apply Sort
    /// </summary>
    /// <param name="sortCriterias"></param>
    /// <returns></returns>
    Task ApplySort(List<SortCriteria> sortCriterias);
    void OnTabSelected(ISelectionItem selectionItem);
}
