using Microsoft.AspNetCore.Components;
using Microsoft.Maui;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.ReturnOrder.BL.Classes;
using Winit.Modules.ReturnOrder.BL.Interfaces;
using Winit.Modules.ReturnOrder.Model.Constants;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.UIInterfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Services;
using WINITMobile.Models.TopBar;
using WINITMobile.Pages.Base;
using WINITSharedObjects.Enums;
namespace WINITMobile.Pages.Return;

public partial class CreateReturnOrder : BaseComponentBase
{
    [Parameter]
    public bool IsNewOrder { get; set; } = true;
    public bool IsOrderPage { get; set; }
    public bool IsPreviewPage { get; set; }
    public bool IsLoaded = false;
    public bool IsInvoiceList = false;
    public List<ISelectionItem> SkuSelectionItems { get; set; }
    private ReturnOrderListView ReturnOrderListView { get; set; }
    public List<ISelectionItem> UOMSelectionItems { get; set; }
    Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderAppViewModel _returnOrderViewModel;

    private WINITMobile.Pages.DialogBoxes.AddProductDialogBox<IReturnOrderItemView> AddProductDialogBox;

    private bool IsWithInvoice = false;
    [CascadingParameter] public EventCallback<Models.TopBar.MainButtons> Btnname { get; set; }
    public List<ISelectionItem> InvoiceSelectionItems = new List<ISelectionItem>()
    {
        new SelectionItem
        {
            UID = "WithInvoice",
            Label ="With Invoice",
            Code = "WithInvoice"
        },new SelectionItem
        {
            UID = "WithoutInvoice",
            Label ="Without Invoice",
            Code = "WithoutInvoice"
        },
    };

    protected override async Task OnInitializedAsync()
    {
        try
        {
            await _dropdownService.ShowMobilePopUpDropDown(new DropDownOptions
            {
                DataSource = InvoiceSelectionItems,
                OnSelect = async (eventArgs) =>
                {
                    await OnInvoiceTypeSelection(eventArgs);
                },
                OkBtnTxt = @Localizer["proceed"],
                Title = @Localizer["Invoice Type"]
            });
        }
        catch (Exception)
        {
            HideLoader();
        }
        LoadResources(null, _languageService.SelectedCulture);
    }
    private async Task OnInvoiceTypeSelection(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent == null || dropDownEvent.SelectionItems == null || !dropDownEvent.SelectionItems.Any()) return;
        Winit.Modules.Store.Model.Interfaces.IStoreItemView storeViewModel =
                   (Winit.Modules.Store.Model.Interfaces.IStoreItemView)_dataManager.GetData(nameof(Winit.Modules.Store.Model.Interfaces.IStoreItemView));
        if (storeViewModel is null) throw new Exception("storeViewModel is null");
        if (dropDownEvent.SelectionItems.First().Code == "WithInvoice")
        {
            _returnOrderViewModel = (IReturnOrderAppViewModel)_viewModelFactory.CreateReturnOrderViewModel(ReturnOrderType.WithInvoice, SourceType.APP);
            _returnOrderViewModel.SelectedStoreViewModel = storeViewModel;
            await (_returnOrderViewModel as IReturnOrderWithInvoiceViewModel).SetSalesOrdersForInvoiceList(_returnOrderViewModel.SelectedStoreViewModel.StoreUID);
            IsInvoiceList = true;
            IsWithInvoice = true;
        }
        else
        {
            IsWithInvoice = false;
            _returnOrderViewModel = (IReturnOrderAppViewModel)_viewModelFactory.CreateReturnOrderViewModel(ReturnOrderType.WithoutInvoice, SourceType.APP);
            _returnOrderViewModel.SelectedStoreViewModel = storeViewModel;
            await PopulateViewModelWithOutInovice();
        }
        StateHasChanged();
    }
    private async Task PopulateViewModelWithOutInovice()
    {
        ShowLoader();
        try
        {
            await _returnOrderViewModel.PopulateViewModel(SourceType.APP, IsNewOrder);
            IsLoaded = true;
        }
        catch
        {
        }
        HideLoader();
    }
    private void OnPreviewBtnClick()
    {
        if (ValidateReturnOrder())
        {
            _navigationManager.NavigateTo("ReturnOrderPreviewPage");
        }
    }
    private bool ValidateReturnOrder()
    {
        if (_returnOrderViewModel.DisplayedReturnOrderItemViews.Count == 0)
        {
            _ = _alertService.ShowErrorAlert(@Localizer["return_order"], @Localizer["no_items_selected"]);
            return false;
        }
        if (_returnOrderViewModel.DisplayedReturnOrderItemViews.Count !=
           _returnOrderViewModel.DisplayedReturnOrderItemViews.Where(e => e.OrderQty > 0).Count())
        {
            _alertService.ShowErrorAlert(@Localizer["return_order"], "Enter qty for all the items");
            return false;
        }
        if (_returnOrderViewModel.DisplayedReturnOrderItemViews.Any(e => string.IsNullOrEmpty(e.ReasonText)))
        {
            _alertService.ShowErrorAlert(@Localizer["return_order"], "Selected reason for all the Items");
            return false;
        }
        _PageState.ReturnOrderViewModel = _returnOrderViewModel;
        return true;
    }
    // Implement IDisposable for object cleanup
    public void Dispose()
    {

    }

    public void AddProductClick(List<IReturnOrderItemView> returnOrderItemViews)
    {
        AddProductDialogBox.OnCloseClick();
        _returnOrderViewModel.AddProductToGrid(returnOrderItemViews);
        StateHasChanged();
    }
    public void CancelClickOnAddProductPopUp()
    {
        AddProductDialogBox.OnCloseClick();
    }
    public void ItemSearch(string searchValue)
    {
        _ = _returnOrderViewModel.ApplySearch(searchValue);
    }
    public async Task QtyChanged(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView returnOrderItemView)
    {
        await _returnOrderViewModel.OnQtyChange(returnOrderItemView);
    }
    public void ReasonTypeChanged((Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView,
        Winit.Shared.Models.Enums.StockType) data)
    {
        IReturnOrderItemView returnOrderItemView = data.Item1;
        StockType stockType = data.Item2;
        returnOrderItemView.ReasonsList = _returnOrderViewModel.GetReasons(stockType);
    }
    public void DeleteItem(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView returnOrderItemView)
    {
        _returnOrderViewModel?.DeleteItem(returnOrderItemView);
    }
    public void UOMDDClicked(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView returnOrderItemView)
    {
        UOMSelectionItems = _returnOrderViewModel.GetAvailableUOMForDDL(returnOrderItemView);
    }
    public async Task DropDownSelection_OnSingleSelect(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
        {
            Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView returnOrderItemView =
                _returnOrderViewModel.ReturnOrderItemViews
                .Where(e => e.UID == dropDownEvent.UID)
                .FirstOrDefault();
            if (returnOrderItemView != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
            {
                Winit.Shared.Models.Common.ISelectionItem selectionItem = dropDownEvent.SelectionItems.FirstOrDefault();
                if (selectionItem != null)
                {
                    returnOrderItemView.SelectedUOM = returnOrderItemView.AllowedUOMs.Find(uom => uom.Code ==
                    selectionItem.Code);
                    await _returnOrderViewModel.OnQtyChange(returnOrderItemView);
                }
            }
        }
        StateHasChanged();
    }
    private async Task OnImageCapture((string fileName, string folderPath) data)
    {
        IFileSys fileSys = ConvertFileSys("ReturnOrder", _returnOrderViewModel.ReturnOrderUID, "Item", "Image",
            data.fileName, _appUser.Emp?.Name, data.folderPath);
        (_returnOrderViewModel as ReturnOrderAppViewModel).ImageFileSysList.Add(fileSys);
        await Task.CompletedTask;
    }
    private void OnImageDeleteClick(string fileName)
    {
        IFileSys fileSys = (_returnOrderViewModel as ReturnOrderAppViewModel).ImageFileSysList.Find
            (e => e.FileName == fileName);
        if (fileSys is not null) (_returnOrderViewModel as ReturnOrderAppViewModel).ImageFileSysList.Remove(fileSys);
    }

    private async Task OnInvoiceSelction()
    {
        try
        {
            ShowLoader();
            if ((_returnOrderViewModel as IReturnOrderWithInvoiceViewModel).SelectedInvoice == null) return;
            IsInvoiceList = false;
            Winit.Modules.Store.Model.Interfaces.IStoreItemView storeViewModel =
                       (Winit.Modules.Store.Model.Interfaces.IStoreItemView)_dataManager.GetData("SelectedStoreViewModel");
            _returnOrderViewModel.SelectedStoreViewModel = storeViewModel ?? throw new Exception("storeViewModel is null");
            await (_returnOrderViewModel as IReturnOrderWithInvoiceViewModel).PopulateViewModelWithInvoice(SourceType.APP, true);
            IsLoaded = true;
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert("Error", ex.Message);
        }
        HideLoader();
    }
}
