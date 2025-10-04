using Microsoft.AspNetCore.Components;
using Nest;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Modules.SalesOrder.BL.UIInterfaces;
using Winit.Modules.SalesOrder.Model.Event;
using Winit.Modules.SalesOrder.Model.UIInterfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Events;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using WinIt.Pages.Base;

namespace WinIt.Pages.SalesOrder;

public partial class CashSalesOrderBasePage : BaseComponentBase
{
    [CascadingParameter]
    public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
    private bool IsNewOrder = true;
    private bool IsInitialized { get; set; }
    private bool IsStoreSelected { get; set; }
    private string? SalesType { get; set; }
    private string? pageTitle { get; set; }
    private string? SelectedRoute { get; set; }
    public List<Winit.Shared.Models.Common.ISelectionItem>? UOMselectionItems { get; set; }
    public List<Winit.Shared.Models.Common.ISelectionItem>? UOMCloneselectionItems { get; set; }
    private Winit.Modules.SalesOrder.BL.UIInterfaces.ISalesOrderWebViewModel _SalesOrderViewModel { get; set; }
    private readonly List<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView> DisplyedSalesOrderItemViews =
        new();
    private WinIt.Pages.DialogBoxes.AddProductDialogBoxV1<ISKUV1>? AddProductDialogBox;
    private bool IsBasePriceRequired { get; set; }
    IDataService dataService = new DataServiceModel();
    void SetHeaderName()
    {

        dataService.HeaderText = IsNewOrder ? "Create Presales Order" : "View/Edit Presales Order";
        dataService.BreadcrumList = new List<IBreadCrum>()
        {
            new BreadCrumModel(){SlNo=1,Text="Manage Presales Order",IsClickable=true,URL="ManagePresalesOrders"},
            new BreadCrumModel(){SlNo=1,Text=dataService.HeaderText},
        };
    }


    bool DisableAfterFinalize { get; set; } = false;
    protected override async Task OnInitializedAsync()
    {
        try
        {
            ShowLoader();
            LoadResources(null, _languageService.SelectedCulture);
            switch (_commonFunctions.GetParameterValueFromURL("SalesType"))
            {
                case "CS":
                    SalesType = OrderType.Cashsales; pageTitle = @Localizer["cash_sales"]; break;
                case "PS":
                    SalesType = OrderType.Presales; pageTitle = @Localizer["pre_sales"]; break;
                case "VS":
                    SalesType = OrderType.Vansales; pageTitle = @Localizer["van_sales"]; break;
                default:
                    SalesType = OrderType.Cashsales; pageTitle = @Localizer["cash_sales"];
                    IsBasePriceRequired = true;
                    break;
            }
            _SalesOrderViewModel = (ISalesOrderWebViewModel)_SalesOrderViewModelFactory.CreateSalesOrderViewModel(SalesType);
            _SalesOrderViewModel.OrderType = SalesType;
            string SalesOrderUID = _commonFunctions.GetParameterValueFromURL("SalesOrderUID");
            if (!string.IsNullOrEmpty(SalesOrderUID))
            {
                IsNewOrder = false;
            }
            else
            {

                _SalesOrderViewModel.OrderDate = DateTime.Now;
            }
            _SalesOrderViewModel._JS = _JS;
            SetHeaderName();
            Task.Run(() => GetSKUAttributeData());
            await _SalesOrderViewModel.PopulateViewModel(Winit.Shared.Models.Constants.SourceType.CPE, default, IsNewOrder, SalesOrderUID);
            IsInitialized = true;
            HideLoader();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            HideLoader();
        }
        finally
        {
            
                DisableAfterFinalize = IsNewOrder?false:(_SalesOrderViewModel.Status == SalesOrderStatus.APPROVED ||
               _SalesOrderViewModel.Status == SalesOrderStatus.FINALIZED ||
               _SalesOrderViewModel.Status == SalesOrderStatus.REJECTED);
           
          
        }
    }
    async Task GetSKUAttributeData()
    {
        var data = await _addProductPopUpDataHelper.GetSKUAttributeData();
        if (data != null)
        {
            _SalesOrderViewModel.SKUAttributeData.AddRange(data!);
        }
    }

    private async Task OnRouteSelected(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any() && _SalesOrderViewModel != null)
        {
            ShowLoader();
            SelectedRoute = dropDownEvent.SelectionItems.FirstOrDefault()?.UID;
            if (SelectedRoute != null)
            {
                await _SalesOrderViewModel.OnRouteSelect(SelectedRoute);
                StateHasChanged();
            }
            HideLoader();
        }
        else
        {
        }
    }
    private async Task OnStoreSelected(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any() && _SalesOrderViewModel != null)
        {
            ShowLoader();
            await _SalesOrderViewModel.OnStoreItemViewSelected(dropDownEvent.SelectionItems.First().UID);
            IsStoreSelected = true;
            HideLoader();
        }
        else
        {
            IsStoreSelected = false;
        }
        StateHasChanged();
    }
    private void OnStoreDistributionChannelSelect(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any()
            && !string.IsNullOrEmpty(dropDownEvent.SelectionItems.First().Code))
        {
            _SalesOrderViewModel.OnStoreDistributionChannelSelect(dropDownEvent.SelectionItems.First().Code!);
        }
        StateHasChanged();
    }
    private async Task OnAddProductClick(List<ISKUV1> sKUs)
    {
        ShowLoader();
        await _SalesOrderViewModel.OnAddProductClick(skus: sKUs);
        var codes = sKUs.FindAll(p => _SalesOrderViewModel.SalesOrderItemViews.Any(q => q.SKUUID == p.UID)).
            Select(e => e.Code).ToList();
        if (codes.Count == 0)
        {
            _alertService.ShowErrorAlert(@Localizer["alert"], $"{@Localizer["product"]}'s {string.Join(",", codes)} price not there");
        }
        StateHasChanged();
        HideLoader();
    }
    private async Task OnaddProductClose(List<ISalesOrderItemView> salesOrderItemViews)
    {
        if (salesOrderItemViews != null && salesOrderItemViews.Any() && _SalesOrderViewModel != null)
        {
            await _SalesOrderViewModel.AddSelectedProducts(salesOrderItemViews);
            StateHasChanged();
        }
    }
    public async Task AddItemToList(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView clonedItem)
    {
        if (_SalesOrderViewModel == null)
        {
            return;
        }

        if (_SalesOrderViewModel != null)
        {
            clonedItem.Qty = 1;
            await _SalesOrderViewModel.AddClonedItemToList(clonedItem);
            StateHasChanged();
        }
    }
    public async Task OnUOMSelection(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any() && _SalesOrderViewModel != null)
        {
            ISelectionItem? selectedItem = dropDownEvent.SelectionItems.FirstOrDefault();
            string UID = dropDownEvent.UID;
            Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView? salesOrderItemView = _SalesOrderViewModel.SalesOrderItemViews.Find(item => item.UID == UID);
            if (salesOrderItemView != null && selectedItem != null)
            {
                salesOrderItemView.SelectedUOM = salesOrderItemView.AllowedUOMs.Find(roiv => roiv.Code == selectedItem.Code);
                await _SalesOrderViewModel.UpdateUnitPriceByUOMChange(salesOrderItemView);
            }
        }
        StateHasChanged();
    }
    public void GetCloneUOMSForSelectedProduct(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView)
    {
        if (_SalesOrderViewModel != null)
        {
            UOMCloneselectionItems = _SalesOrderViewModel.GetAvailableUOMForClone(salesOrderItemView);
        }
    }
    public async Task DeleteClonedItem(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView)
    {
        if (await _alertService.ShowConfirmationReturnType(@Localizer["delete"], @Localizer["are_you_sure_you_want_to_delete_this_item_?"], @Localizer["yes"], @Localizer["no"]) && _SalesOrderViewModel != null)
        {
            await _SalesOrderViewModel.RemoveItemFromList(salesOrderItemView);
            StateHasChanged();
        }
    }
    public async Task QtyChanged(SalesOrderItemEvent salesOrderItemEvent)
    {
        if (_SalesOrderViewModel == null)
        {
            return;
        }

        salesOrderItemEvent.SalesOrderItemView.Qty = salesOrderItemEvent.Qty;
        await _SalesOrderViewModel.OnQtyChange(salesOrderItemEvent.SalesOrderItemView);
        StateHasChanged();
    }
    public async Task OnUnitPriceChange(SalesOrderItemEvent salesOrderItemEvent)
    {
        if (_SalesOrderViewModel == null)
        {
            return;
        }

        await _SalesOrderViewModel.UpdateUnitPrice(salesOrderItemEvent.SalesOrderItemView, salesOrderItemEvent.UnitPrice);
        StateHasChanged();
    }
    public void GetUOMSForSelectedProduct(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView)
    {
        if (_SalesOrderViewModel == null)
        {
            return;
        }

        UOMselectionItems = _SalesOrderViewModel.GetAvailableUOMForDDL(salesOrderItemView);
    }
    private async Task Handel_deleteSelectedItems()
    {
        if (await _alertService.ShowConfirmationReturnType(@Localizer["delete"], @Localizer["are_you_sure_you_want_to_delete_this_item_?"], @Localizer["yes"], @Localizer["no"]))
        {
            //DisplyedSalesOrderItemViews.RemoveAll(e => e.IsSelected);
            if (_SalesOrderViewModel == null)
            {
                return;
            }

            await _SalesOrderViewModel.ClearCartItems(_SalesOrderViewModel.SalesOrderItemViews.FindAll(e => e.IsSelected));
            StateHasChanged();
        }
    }
    private bool ValideSalesOrder()
    {
        if (_SalesOrderViewModel == null)
        {
            return false;
        }

        if (string.IsNullOrEmpty(_SalesOrderViewModel.OrderType))
        {
            ShowErrorSnackBar(pageTitle, @Localizer["sales_type_is_not_selected"]);
            return false;
        }
        else if (string.IsNullOrEmpty(_SalesOrderViewModel.RouteUID))
        {
            ShowErrorSnackBar(pageTitle, @Localizer["route_is_not_selected"]);
            return false;
        }
        else if (_SalesOrderViewModel.SelectedStoreViewModel is null)
        {
            ShowErrorSnackBar(pageTitle, @Localizer["store_is_not_selected"]);
            return false;
        }
        else if (IsBasePriceRequired && (string.IsNullOrEmpty(_SalesOrderViewModel.CustomerName) || string.IsNullOrEmpty(_SalesOrderViewModel.Address)))
        {
            ShowErrorSnackBar(pageTitle, @Localizer["some_fields_are_missing"]);
            return false;
        }
        else if (!_SalesOrderViewModel.SalesOrderItemViews.Any())
        {
            ShowErrorSnackBar(pageTitle, @Localizer["add_atleast_one_item"]);
            return false;
        }
        else if (_SalesOrderViewModel.SalesOrderItemViews.Any(e => e.Qty <= 0))
        {
            ShowErrorSnackBar(pageTitle, "please enter the quantity ");
            return false;
        }
        return true;
    }
    private async Task HandleSave_SaveClick()
    {
        if (ValideSalesOrder() && await _alertService.ShowConfirmationReturnType(@Localizer["save"], @Localizer["are_you_sure_you_want_to_save_this_order"], @Localizer["yes"], @Localizer["no"]) && _SalesOrderViewModel != null)
        {
            try
            {
                if (await _SalesOrderViewModel.SaveSalesOrder(SalesOrderStatus.DRAFT))
                {
                    ShowSuccessSnackBar(pageTitle, @Localizer["saved_successfully"]);
                    if (SalesType == OrderType.Presales)
                    {
                        _navigationManager.NavigateTo("ManagePresalesOrders");
                    }
                    else
                    {
                        _navigationManager.NavigateTo("ManageUnallocatedOrders");
                    }
                }
                else
                {
                    ShowErrorSnackBar(pageTitle, @Localizer["failed_to_save"]);
                }
            }
            catch (CustomException ex)
            {
                ShowErrorSnackBar(Enum.GetName(ex.Status), ex.Message);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
    private string GetSalesOrderStatusBySalesType(string? salesType)
    {
        return salesType switch
        {
            OrderType.Presales => SalesOrderStatus.FINALIZED,
            OrderType.Vansales or OrderType.Cashsales => SalesOrderStatus.DELIVERED,
            _ => SalesOrderStatus.FINALIZED,
        };
    }
    private async Task HandleSaveAndFinalize_SaveAndFinalizeClick()
    {
        if (ValideSalesOrder() && await _alertService.ShowConfirmationReturnType(@Localizer["save_&_finalize"], @Localizer["are_you_sure_you_want_to_finalize_this_order"], @Localizer["yes"], @Localizer["no"]) && _SalesOrderViewModel != null)
        {
            try
            {
                if (await _SalesOrderViewModel.SaveSalesOrder(GetSalesOrderStatusBySalesType(SalesType)))
                {
                    ShowSuccessSnackBar(pageTitle, @Localizer["order_placed_successfully"]);
                    if (SalesType == OrderType.Presales)
                    {
                        _navigationManager.NavigateTo("ManagePresalesOrders");
                    }
                    else
                    {
                        _navigationManager.NavigateTo("ManageUnallocatedOrders");
                    }
                }
                else
                {
                    ShowErrorSnackBar(pageTitle, @Localizer["operation_failed..."]);
                }
            }
            catch (CustomException ex)
            {
                ShowErrorSnackBar(Enum.GetName(ex.Status), ex.Message);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    private async Task UpdateSalesOrderStatus(string status)
    {
        if (await _alertService.ShowConfirmationReturnType(@Localizer["confirm"], $"{@Localizer["are_you_sure_you_want_to"]} {status} {@Localizer["this_order"]}", @Localizer["yes"], @Localizer["no"]) && _SalesOrderViewModel != null)
        {
            if (await _SalesOrderViewModel.UpdateSalesOrderStatus(status))
            {
                ShowSuccessSnackBar(@Localizer["success"], $"{@Localizer["sales_order_successfully"]} {status}");
                _navigationManager.NavigateTo("ManagePresalesOrders");
            }
            else
            {
                ShowErrorSnackBar(@Localizer["error"], $"{@Localizer["failed_to"]} {status} {@Localizer["this_order"]}");
            }
        }
    }
}

