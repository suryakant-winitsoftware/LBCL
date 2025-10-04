using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SalesOrder.Model.UIInterfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using WINITSharedObjects.Constants;

namespace Winit.Modules.SalesOrder.BL.UIClasses;

public class SalesSummaryBaseViewModel : UIInterfaces.ISalesSummaryViewModel
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<ISelectionItem> TabList { get; set; }
    public ISelectionItem? SelectedTab { get; set; }
    public Dictionary<string, List<Model.UIInterfaces.ISalesSummaryItemView>> SalesSummaryItemViewsDictionary { get; set; }
    public List<Model.UIInterfaces.ISalesSummaryItemView>? DisplayedSalesSummaryItemViews { get; set; }

    public SalesSummaryBaseViewModel()
    {
        StartDate = DateTime.Now.Date.AddDays(-7);
        EndDate = DateTime.Now.Date;
        TabList = new List<ISelectionItem>
        {
            new SelectionItem { Code = SalesSummaryTabs.Todays, Label = "Today's" },
            new SelectionItem { Code = SalesSummaryTabs.All, Label = "ALL" },
            //new SelectionItem { Code = SalesSummaryTabs.Old, Label = "Old" },
            //new SelectionItem { Code = SalesSummaryTabs.Draft, Label = "Draft" }
        };
        SalesSummaryItemViewsDictionary = new Dictionary<string, List<ISalesSummaryItemView>>();
    }
    public void PopulateViewModel(Winit.Modules.SalesOrder.BL.Interfaces.ISalesOrderBL salesOrderBL)
    {
        PopulateSalesSummaryItemViewsDictionary(salesOrderBL);
        DisplayedSalesSummaryItemViews = new List<Model.UIInterfaces.ISalesSummaryItemView>();
    }
    

    public virtual async Task PopulateSalesSummaryItemViewsDictionary(Winit.Modules.SalesOrder.BL.Interfaces.ISalesOrderBL salesOrderBL)
    {
        SalesSummaryItemViewsDictionary.Clear();
        foreach(var item in await salesOrderBL.GetISalesSummaryItemViews((DateTime)StartDate, (DateTime)EndDate)){
            SalesSummaryItemViewsDictionary.Add(item.Key, item.Value);
        }
    }

    public void Dispose()
    {
        // Dispose any object references if needed
    }

    public async Task ApplySearch(string searchString)
    {
        if (SelectedTab != null && SalesSummaryItemViewsDictionary.ContainsKey(SelectedTab.Code))
        {
            DisplayedSalesSummaryItemViews = SalesSummaryItemViewsDictionary[SelectedTab.Code]
                .Where(item => item.StoreName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    public async Task ApplySort(List<SortCriteria> sortCriterias)
    {
        // Implement sorting logic
    }

    public void OnTabSelected(ISelectionItem selectionItem)
    {
        DisplayedSalesSummaryItemViews?.Clear();
        if (selectionItem != null)
        {
            SelectedTab = selectionItem;
            if (SalesSummaryItemViewsDictionary.ContainsKey(selectionItem.Code))
            {
                DisplayedSalesSummaryItemViews?.AddRange(SalesSummaryItemViewsDictionary[selectionItem.Code]);
            }
        }
    }
}
