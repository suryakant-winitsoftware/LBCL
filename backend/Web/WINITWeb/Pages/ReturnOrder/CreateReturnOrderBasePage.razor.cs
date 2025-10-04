using MathNet.Numerics.Optimization.TrustRegion;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using NPOI.HSSF.Record.Aggregates.Chart;
using NPOI.OpenXmlFormats.Dml;
using System.Globalization;
using System.Resources;
using Winit.Modules.ReturnOrder.BL.Interfaces;
using Winit.Modules.ReturnOrder.Model.Constants;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.SKU.Model.UIInterfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;

using Winit.UIComponents.Common.Language;
using WinIt.Pages.Base;



namespace WinIt.Pages.ReturnOrder;

partial class CreateReturnOrderBasePage : BaseComponentBase
{
    [CascadingParameter]
    public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
    [Parameter]
    public bool IsViewMode { get; set; }
    [Parameter]
    public string? ReturnOrderUID { get; set; }
    [Parameter]
    public bool IsWithInvoice { get; set; }
    Winit.UIComponents.Common.DialogBoxes.AddProductDialogBox addProductDialogBoxReffrenece;
    public List<Winit.Shared.Models.Common.ISelectionItem> UOMCloneselectionItems = new List<ISelectionItem>();
    private Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderWebViewModel _returnOrderViewModel;
    private bool IsInitialized { get; set; }
    private bool IsShowOrganazation = false;
    private bool IsInvoiceList = false;
    private WinIt.Pages.DialogBoxes.AddProductDialogBox<IReturnOrderItemView>? AddProductDialogBox;
    IDataService dataService = new DataServiceModel()
    {
        HeaderText = "Create Return Order",
        BreadcrumList = new List<IBreadCrum>()
         {
             new BreadCrumModel(){SlNo=1,Text="Create Return Order"},
         }
    };

    protected override async Task OnInitializedAsync()
    {
        try
        {
            LoadResources(null, _languageService.SelectedCulture);
            ShowLoader();
           
            if (!string.IsNullOrEmpty(ReturnOrderUID))
            {

                if (IsWithInvoice)
                {
                    dataService.HeaderText = $"{"Create Return Order With Invoice"}";
                    _returnOrderViewModel = (IReturnOrderWebViewModel)_viewModelFactory.CreateReturnOrderViewModel(ReturnOrderType.WithInvoice, SourceType.CPE);
                    await (_returnOrderViewModel as IReturnOrderWithInvoiceViewModel).PopulateViewModelWithInvoice(SourceType.CPE, false, ReturnOrderUID);
                }
                else
                {
                    dataService.HeaderText = $"{"Create Return Order Without Invoice"}"; 
                    _returnOrderViewModel = (IReturnOrderWebViewModel)_viewModelFactory.CreateReturnOrderViewModel(ReturnOrderType.WithoutInvoice, SourceType.CPE);
                    await GetExistingOrder();
                }
            }
            else
            {
                if (IsWithInvoice)
                {
                    dataService.HeaderText = $"{"Create Return Order With Invoice"}";
                    _returnOrderViewModel = (IReturnOrderWebViewModel)_viewModelFactory.CreateReturnOrderViewModel(ReturnOrderType.WithInvoice, SourceType.CPE);
                    await (_returnOrderViewModel as IReturnOrderWithInvoiceViewModel).PopulateViewModelWithInvoice(SourceType.CPE, true);
                }
                else
                {

                    dataService.HeaderText = $"{"Create Return Order Without Invoice"}"; 
                    _returnOrderViewModel = (IReturnOrderWebViewModel)_viewModelFactory.CreateReturnOrderViewModel(ReturnOrderType.WithoutInvoice, SourceType.CPE);
                    await _returnOrderViewModel.PopulateViewModel(SourceType.CPE);
                }
            }
            IsInitialized = true;
            HideLoader();
        }
        catch (Exception ex)
        {
            
        }
        finally
        {
            HideLoader();
            StateHasChanged();
        }
    }

    private async Task GetExistingOrder()
    {
        //Winit.Modules.Store.Model.Interfaces.IStoreItemView storeViewModel = 
        await _returnOrderViewModel.PopulateViewModel(SourceType.CPE, false, ReturnOrderUID);
        IsInitialized = true;
    }

    private async Task OnRouteSelected(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            var selectedRoute = dropDownEvent.SelectionItems.First();
            selectedRoute.IsSelected = true;
            await _returnOrderViewModel.OnRouteSelect(selectedRoute.UID);
        }
        StateHasChanged();
    }
    private async Task OnStoreSelected(DropDownEvent dropDownEvent)
    {
        try
        {
            if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                ShowLoader();
                await _returnOrderViewModel.OnStoreItemViewSelected(dropDownEvent.SelectionItems.First().UID);
                addProductDialogBoxReffrenece?.InitialTableRows();
                if (_returnOrderViewModel.StoreCreditsSelectionItems != null && _returnOrderViewModel.StoreCreditsSelectionItems.Any())
                {
                    IsShowOrganazation = true;
                }
                else
                {
                    IsShowOrganazation = false;
                }
                HideLoader();
            }
            else
            {
            }
            StateHasChanged();
        }
        catch (Exception ex)
        {

        }
    }

    public void OnStoreDistributionChannelSelect(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {

            _returnOrderViewModel.OnStoreDistributionChannelSelect(dropDownEvent.SelectionItems.First().Code!);
        }
    }
    private List<ISelectionItem> ConvertStoreToSelectionItem(IEnumerable<Winit.Modules.Store.Model.Interfaces.IStore> stores)
    {
        List<ISelectionItem> selectionItems = new List<ISelectionItem>();
        foreach (var store in stores)
        {
            ISelectionItem si = new SelectionItem();
            si.Label = store.Name;
            si.Code = store.Code;
            si.UID = store.UID;
            selectionItems.Add(si);
        }
        return selectionItems;
    }
    private async Task SaveAndApprove()
    {
        try
        {
            _returnOrderViewModel.Validate();
            if (await _alertService.ShowConfirmationReturnType(@Localizer["save_&_approve"], @Localizer["are_you_sure_you_want_to_save_and_approve"]))
            {
                _returnOrderViewModel.Status = Winit.Shared.Models.Constants.SalesOrderStatus.APPROVED;
                if (await _returnOrderViewModel.SaveOrder())
                {
                    ShowSuccessSnackBar(@Localizer["success"], @Localizer["order_approved_successfully"]);
                    _navigationManager.NavigateTo("ReturnOrderSummary");
                }
                else
                {
                    ShowErrorSnackBar(@Localizer["error"], @Localizer["order_failed_to_save&approve"]);
                }
            }
        }
        catch (CustomException ex)
        {
            ShowErrorSnackBar("Failed", ex.Message);
        }
        catch (Exception)
        {
            throw;
        }
    }
    private async Task Save()
    {
        try
        {
            _returnOrderViewModel.Validate();
            if (await _alertService.ShowConfirmationReturnType(@Localizer["save"], @Localizer["are_you_sure_you_want_to_save_?"], @Localizer["yes"], @Localizer["no"]))
            {
                _returnOrderViewModel.Status = Winit.Shared.Models.Constants.SalesOrderStatus.DRAFT;
                if (await _returnOrderViewModel.SaveOrder())
                {
                    ShowSuccessSnackBar(@Localizer["success"], @Localizer["order_saved_successfully"]);
                    _navigationManager.NavigateTo("ReturnOrderSummary");
                }
                else
                {
                    ShowErrorSnackBar(@Localizer["error"], @Localizer["order_failed_to_save"]);
                }
            }
        }
        catch (CustomException ex)
        {
            ShowErrorSnackBar("Failed", ex.Message);
        }
        catch (Exception ex)
        {

        }
    }
    public void SkuTypeChanged((Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView, Winit.Shared.Models.Enums.StockType) data)
    {
        IReturnOrderItemView returnOrderItemView = data.Item1;
        StockType stockType = data.Item2;
        returnOrderItemView.ReasonsList = _returnOrderViewModel.GetReasons(stockType);
    }
    public void AddItemToList(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView clonedItem)
    {
        _returnOrderViewModel.AddItemToList(_returnOrderViewModel.ReturnOrderItemViews, clonedItem, false);
        _returnOrderViewModel.AddItemToList(_returnOrderViewModel.DisplayedReturnOrderItemViews, clonedItem, false);
        _returnOrderViewModel.AddItemToList(_returnOrderViewModel.FilteredReturnOrderItemViews, clonedItem, false);
    }
    public void OnUOMSelection(DropDownEvent dropDownEvent)
    {
        var selectedItem = dropDownEvent.SelectionItems.FirstOrDefault();
        string UID = dropDownEvent.UID;
        var returnOrderItemView = _returnOrderViewModel.ReturnOrderItemViews.Find(item => item.UID == UID);
        returnOrderItemView.SelectedUOM = returnOrderItemView.AllowedUOMs.Find(roiv => roiv.Code == selectedItem.Code);
        _returnOrderViewModel.UpdateItemPrice(returnOrderItemView);
    }
    public List<ISelectionItem> GetUOMSForSelectedProduct(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView returnOrderItemView)
    {
        return _returnOrderViewModel.GetAvailableUOMForDDL(returnOrderItemView);
    }
    public async Task GetCloneUOMSForSelectedProduct(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView returnOrderItemView)
    {
        UOMCloneselectionItems.Clear();
        await _dropdownService.ShowDropDown(new Winit.UIComponents.Common.Services.DropDownOptions
        {
            DataSource = _returnOrderViewModel.GetAvailableUOMForClone(returnOrderItemView),
            OnSelect = async (eventArgs) =>
            {
                // Your asynchronous handling logic goes here
                await OnCloneUomSelect(eventArgs, returnOrderItemView);
            },
        });
    }
    private async Task OnCloneUomSelect(Winit.Shared.Models.Events.DropDownEvent dropDownEvent, IReturnOrderItemView returnOrderItemView)
    {
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            ISelectionItem selectionitem = dropDownEvent.SelectionItems.First();
            if (returnOrderItemView.UsedUOMCodes is null) returnOrderItemView.UsedUOMCodes = new List<string>();
            returnOrderItemView.UsedUOMCodes.Add(selectionitem.Code);
            ISKUUOMView newsKUUOM = returnOrderItemView.AllowedUOMs.Find(uom => uom.Code == selectionitem.Code)!;
            var cloneditem = returnOrderItemView.Clone(newsKUUOM, ItemState.Cloned, newsKUUOM.Code);
            cloneditem.SelectedUOM = newsKUUOM;
            AddItemToList(cloneditem);
        }
        StateHasChanged();
        await Task.CompletedTask;
    }
    public async Task DeleteClonedItem(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView returnOrderItemView)
    {
        if (await _alertService.ShowConfirmationReturnType(@Localizer["delete"], @Localizer["are_you_sure_you_want_to_delete_selected_item"], @Localizer["yes"], @Localizer["no"]))
        {
            _returnOrderViewModel.DeleteClonedItem(returnOrderItemView);
            StateHasChanged();
        }
    }
    public async Task QtyChanged(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView returnOrderItemView)
    {
        await _returnOrderViewModel.OnQtyChange(returnOrderItemView);
    }
    public void OnaddProductClose(List<IReturnOrderItemView> returnOrderItemViews)
    {
        _returnOrderViewModel.AddProductToGrid(returnOrderItemViews);
        StateHasChanged();
    }
    private async Task OnInvoiceSelction(DropDownEvent dropDownEvent)
    {
        try
        {
            if (dropDownEvent == null || dropDownEvent.SelectionItems == null || !dropDownEvent.SelectionItems.Any()) return;
            ShowLoader();
            (_returnOrderViewModel as IReturnOrderWithInvoiceViewModel)!.SelectedInvoice = (_returnOrderViewModel as IReturnOrderWithInvoiceViewModel)!.
                SalesOrdersInvoices.Find(e => e.SalesOrderUID == dropDownEvent.SelectionItems!.FirstOrDefault()!.UID);
            if ((_returnOrderViewModel as IReturnOrderWithInvoiceViewModel)!.SelectedInvoice == null) return;
            await (_returnOrderViewModel as IReturnOrderWithInvoiceViewModel)!.OnInvoiceSelect();
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert("Error", ex.Message);
        }
        HideLoader();
    }
}
