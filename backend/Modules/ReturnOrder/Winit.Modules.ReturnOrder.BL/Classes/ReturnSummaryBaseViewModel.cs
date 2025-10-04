using Newtonsoft.Json;
using System.Formats.Asn1;
using System.Security.Cryptography;
using Winit.Modules.Common.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.ReturnOrder.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
namespace Winit.Modules.ReturnOrder.BL.Classes;

public abstract class ReturnSummaryBaseViewModel : IReturnSummaryViewModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<ISelectionItem> TabList { get; set; }
    public ISelectionItem? SelectedTab { get; set; }
    public SelectionManager TabSelectionManager { get; set; }
    public Dictionary<string, List<Model.Interfaces.IReturnSummaryItemView>?> ReturnSummaryItemViewsDictionary { get; set; }
    public List<Model.Interfaces.IReturnSummaryItemView> DisplayedReturnSummaryItemViews { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalReturnSummaryItemViews { get; set; }
    protected readonly IAppUser _appUser;

    protected ReturnSummaryBaseViewModel(IAppUser appUser)
    {
        StartDate = DateTime.Now.AddYears(-1);
        EndDate = DateTime.Now.AddYears(1);
        TabList = new List<ISelectionItem>()
            {
                new SelectionItemTab { Code = "Requested", Label = "Requested" },
                new SelectionItemTab { Code = "Approved", Label = "Approved" },
                new SelectionItemTab { Code = "Collected", Label = "Collected" },
            };
        ReturnSummaryItemViewsDictionary = new Dictionary<string, List<Model.Interfaces.IReturnSummaryItemView>>();
        TabSelectionManager = new SelectionManager(TabList, SelectionMode.Single);
        DisplayedReturnSummaryItemViews = new List<Model.Interfaces.IReturnSummaryItemView>();
        _appUser = appUser;
    }
    /// <summary>
    /// Populate Initial Data
    /// </summary>
    public virtual async Task PopulateViewModel()
    {
        await PopulateReturnSummaryItemViewsDictionary();
        OnTabSelected(TabList?.First());
    }
    /// <summary>
    /// Populate the tab list you want to show
    /// </summary>
    public virtual async Task PopulateReturnSummaryItemViewsDictionary(string storeUID = "")
    {
        List<Model.Interfaces.IReturnSummaryItemView>? data = await
            GetReturnOrderSummaryItemViews_Data(StartDate, EndDate, storeUID);
        if (data != null && data.Any())
        {
            ReturnSummaryItemViewsDictionary["Requested"] = data.Where(item => item.OrderStatus == "Draft").ToList();
            ReturnSummaryItemViewsDictionary["Approved"] = data.Where(item => item.OrderStatus == "Approved").ToList();
            ReturnSummaryItemViewsDictionary["Collected"] = data.Where(item => item.OrderStatus == "Collected").ToList();
        }
        else
        {
            ReturnSummaryItemViewsDictionary["Requested"] = null;
            ReturnSummaryItemViewsDictionary["Approved"] = null;
            ReturnSummaryItemViewsDictionary["Collected"] = null;
        }
        foreach (var tab in TabList)
        {
            if (ReturnSummaryItemViewsDictionary[tab.Code!] != null)
            {
                ((SelectionItemTab)tab).Count = ReturnSummaryItemViewsDictionary[tab.Code!]!.Count;
            }
            else
            {
                ((SelectionItemTab)tab).Count = 0;
            }
        }
    }


    /// <summary>
    /// Dispose object Reference
    /// </summary>
    public void Dispose()
    {

    }
    /// <summary>
    /// Apply Search
    /// </summary>
    /// <param name="searchString"></param>
    /// <returns></returns>
    public async Task ApplySearch(string searchString)
    {
        DisplayedReturnSummaryItemViews.Clear();
        DisplayedReturnSummaryItemViews.AddRange(ReturnSummaryItemViewsDictionary[SelectedTab.Code].Where(item => item.StoreName.Contains(searchString)).ToList());
    }
    /// <summary>
    /// Apply Sort
    /// </summary>
    /// <param name="sortCriterias"></param>
    /// <returns></returns>
    public async Task ApplySort(List<SortCriteria> sortCriterias)
    {

    }
    /// <summary>
    /// Apply Filter
    /// </summary>
    /// <param name="filterCriterias"></param>
    /// <returns></returns>
    public void OnTabSelected(ISelectionItem selectionItem)
    {
        TabSelectionManager?.Select(selectionItem);
        if (selectionItem != null)
        {
            SelectedTab = selectionItem;
            DisplayedReturnSummaryItemViews.Clear();
            if (ReturnSummaryItemViewsDictionary != null && ReturnSummaryItemViewsDictionary[selectionItem.Code] != null)
            {
                DisplayedReturnSummaryItemViews.AddRange(ReturnSummaryItemViewsDictionary[selectionItem.Code]);
            }
        }
    }
    public Task PageIndexChanged(int pageIndex)
    {
        return Task.CompletedTask;
    }

    public abstract Task ApplyFilter();

    public async Task<bool> UpdateReturnOrderStatus(string status)
    {
        List<string>? returnOrderUIDs = DisplayedReturnSummaryItemViews?.Where(item => item.IsSelected)?.Select(e => e.UID)?.ToList();
        if (returnOrderUIDs != null)
        {
            return await UpdateReturnOrderStatus_Data(returnOrderUIDs, status);
        }
        else return false;
    }
    #region Abstract Methods
    protected abstract Task<bool> UpdateReturnOrderStatus_Data(List<string> returnOrderUIDs, string Status);
    protected abstract Task<List<Model.Interfaces.IReturnSummaryItemView>> GetReturnOrderSummaryItemViews_Data
        (DateTime startDate, DateTime endDate, string? storeUID = null);
    #endregion
}

