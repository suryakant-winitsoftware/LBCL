using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ReturnOrder.BL.Interfaces;

public interface IReturnSummaryViewModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<ISelectionItem> TabList { get; set; }
    public ISelectionItem? SelectedTab { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalReturnSummaryItemViews { get; set; }
    public Dictionary<string, List<Model.Interfaces.IReturnSummaryItemView>?> ReturnSummaryItemViewsDictionary { get; set; }
    public List<Model.Interfaces.IReturnSummaryItemView> DisplayedReturnSummaryItemViews { get; set; }
    /// <summary>
    /// Populate Initial Data
    /// </summary>
    Task PopulateViewModel();
    Task PopulateReturnSummaryItemViewsDictionary(string storeUID="");
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
    Task PageIndexChanged(int pageIndex);
    Task ApplyFilter();
}
