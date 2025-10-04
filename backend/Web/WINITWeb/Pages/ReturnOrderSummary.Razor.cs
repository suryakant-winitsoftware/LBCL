using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using Winit.Modules.ReturnOrder.BL.Classes;
using Winit.Modules.ReturnOrder.Model.Constants;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Web.Filter;
using Winit.UIModels.Common.Filter;
using WinIt.Pages.Base;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;

namespace WinIt.Pages;

public partial class ReturnOrderSummary : BaseComponentBase
{
    [CascadingParameter]
    public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
    private IReturnSummaryItemView? ViewOrdersList { get; set; }

    private bool IsInitialized = false; // Add a flag to track initialization
    public List<DataGridColumn>? DataGridColumns { get; set; }
    private Filter? FilterRef;
    public List<FilterModel> FilterColumns = new();
    private readonly List<ISelectionItem> OrderTypeSelectionItems = new()
    {
        new SelectionItem{UID=ReturnOrderType.WithInvoice,Code=ReturnOrderType.WithInvoice,Label="With Invoice"},
        new SelectionItem{UID=ReturnOrderType.WithoutInvoice,Code=ReturnOrderType.WithoutInvoice,Label="Without Invoice"},
    };
    IDataService dataService = new DataServiceModel()
    {
        HeaderText = "Return Order Summary",
        BreadcrumList = new List<IBreadCrum>()
         {
             new BreadCrumModel(){SlNo=1,Text="Return Order Summary"},
         }
    };

    protected override async Task OnInitializedAsync()
    {
        try
        {
            LoadResources(null, _languageService.SelectedCulture);
            ShowLoader();
          
            GenerateGridColumns();
            await _ReturnSummeryViewModel.PopulateViewModel();
            FilterColumns.AddRange(new List<FilterModel> {
                new() { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["orderno"],
                    ColumnName = "ReturnOrderNumber" },
                new() { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                    DropDownValues = OrderTypeSelectionItems, ColumnName = "OrderType", Label = @Localizer["order_type"]},
                new() { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                    DropDownValues=(_ReturnSummeryViewModel as ReturnSummaryWebViewModel)!.RouteSelectionItems ,
                    ColumnName = "RouteUID", Label = @Localizer["route"], OnDropDownSelect = OnRouteSelect },
                new() { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, ColumnName = "StoreUID",
                    DropDownValues=(_ReturnSummeryViewModel as ReturnSummaryWebViewModel)!.StoreSelectionItems , Label = @Localizer["customer"]},
                new() { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, Label = @Localizer["return_date"], ColumnName = "OrderDate" },
                new() { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["status"], ColumnName = "Status" }
                });
            IsInitialized = true;
            HideLoader();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            HideLoader();
        }
    }

 
    private async Task OnRouteSelect(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            await (_ReturnSummeryViewModel as ReturnSummaryWebViewModel)!.OnRouteSelect(dropDownEvent.SelectionItems.First().UID);
        }
    }
    private void GenerateGridColumns()
    {
        DataGridColumns = new List<DataGridColumn>
        {
        new() { Header = @Localizer["order_number"], GetValue = s => ((IReturnSummaryItemView)s).OrderNumber },
        new() { Header = @Localizer["order_type"], GetValue = s => ((IReturnSummaryItemView)s).OrderType },
        new() { Header = @Localizer["order_date"], GetValue = s => CommonFunctions.GetDateTimeInFormat(((IReturnSummaryItemView)s).OrderDate)},
        new() { Header =@Localizer["store_code"] , GetValue = s => ((IReturnSummaryItemView)s).StoreCode },
        new() { Header = @Localizer["store_name"], GetValue = s => ((IReturnSummaryItemView)s).StoreName },
        new() { Header = @Localizer["order_status"], GetValue = s => ((IReturnSummaryItemView)s).OrderStatus },
        new() {
                Header = @Localizer["actions"],
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    new() {
                        Text = @Localizer["view"],
                        Action = item => OnActionBtnClick(item, true)
                    },
                    new() {
                        Text = @Localizer["edit"],
                        Action = item => OnActionBtnClick(item, false)
                    }
                }
            }
        };
    }
    private void OnActionBtnClick(object returnSummaryItemView, bool isView)
    {
        string returnOrderUID = ((IReturnSummaryItemView)returnSummaryItemView).UID;

        string url = $@"ReturnOrder/{(((IReturnSummaryItemView)returnSummaryItemView).OrderType == ReturnOrderType.WithInvoice)}/{returnOrderUID}/{isView}";
        _navigationManager.NavigateTo(url);
    }
    private void OnTabSelected(ISelectionItem selectionItem)
    {
        _ReturnSummeryViewModel.OnTabSelected(selectionItem);
    }
    private void OnCheckBoxClick(HashSet<object> hashSet)
    {
        foreach (object item in hashSet)
        {
            (item as IReturnOrderItemView)!.IsSelected = true;
        }
    }
    private async Task OnFilterApply(IDictionary<string, string> keyValuePairs)
    {
        List<FilterCriteria> filterCriterias = new();
        foreach (KeyValuePair<string, string> keyValue in keyValuePairs)
        {
            if (!string.IsNullOrEmpty(keyValue.Value))
            {
                filterCriterias.Add(new FilterCriteria(keyValue.Key, keyValue.Value, FilterType.Equal));
            }
        }
        (_ReturnSummeryViewModel as ReturnSummaryWebViewModel)!.FilterCriterias.Clear();
        (_ReturnSummeryViewModel as ReturnSummaryWebViewModel)!.FilterCriterias.AddRange(filterCriterias);
        await _ReturnSummeryViewModel.ApplyFilter();
        StateHasChanged();
    }
}
