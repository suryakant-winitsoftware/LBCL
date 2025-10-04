using Microsoft.AspNetCore.Components;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.UIInterfaces;
using Winit.Shared.Models.Constants;
using WINITMobile.Models.TopBar;
using WINITMobile.Pages.Base;

namespace WINITMobile.Pages.Sales;

public partial class SalesOrderPreview : SalesOrderBase, IDisposable
{
    public bool IsTrue = false;
    private readonly bool IsCashDiscountPopupVisible = false;
    private bool IsLoaded = false;
    private DiscountPopup refDiscountPopup;
    [CascadingParameter] public EventCallback<MainButtons> Btnname { get; set; }
    protected override void OnInitialized()
    {

    }
    protected override async Task OnInitializedAsync()
    {
        _salesOrderViewModel = (Winit.Modules.SalesOrder.BL.UIInterfaces.ISalesOrderAppViewModel)_dataManager.GetData("salesOrderViewModel");
        if (_salesOrderViewModel != null)
        {
            //await SetTopBar();
            await WinitTextBox_OnSearch("");
            LoadResources(null, _languageService.SelectedCulture);

        }
        IsLoaded = true;
    }
    protected override void OnAfterRender(bool firstRender)
    {
    }

    public async Task WinitTextBox_OnSearch(string searchStringFromComponent)
    {
        await _salesOrderViewModel.ApplySearch_Preview(searchStringFromComponent);
        StateHasChanged();
    }

    #region TopBar
    //private async Task SetTopBar()
    //{
    //    MainButtons buttons = new()
    //    {
    //        UIButton1 = new Buttons() { ButtonType = ButtonType.Image, URL = "/Images/save_btn.png", IsVisible = true, Action = SaveButtonClick },
    //        UIButton2 = new Buttons() { ButtonType = ButtonType.Image, URL = "/Images/print_btn.png", IsVisible = true, Action = PrintClick },
    //        //UIButton3 = new Buttons() { ButtonType = ButtonType.Text, ButtonText = "Cash Disc", IsVisible = true, Action = CashDiscountClick },
    //        TopLabel = @Localizer["sales_order_preview"],
    //        BottomLabel = _salesOrderViewModel.SelectedStoreViewModel.Name
    //    };
    //    await Btnname.InvokeAsync(buttons);
    //}
    public async Task SaveButtonClick()
    {
        if (!ValidateData())
        {
            // Validation errors, show an error popup
            await _alertService.ShowErrorAlert(@Localizer["error"], string.Join(",", ValidationErrors));
        }
        else
        {
            await _alertService.ShowConfirmationAlert(@Localizer["order_confirmation"], "Are you sure you want to confirm the sales capture?", (param) => OnSaveConfirmed());
        }
    }
    public async Task PrintClick()
    {
        await _alertService.ShowSuccessAlert(@Localizer["alert"], @Localizer["work_in_progress"]);
    }
    public void CashDiscountClick()
    {
        refDiscountPopup.Show();
    }
    #endregion
    #region CustomerPO
    private void CustomerPO_OnApply(string customerPO)
    {
        _salesOrderViewModel.UpdateCustomerPO(customerPO);
    }
    #endregion
    #region Notes
    private void Notes_OnApply(string notes)
    {
        _salesOrderViewModel.UpdateNotes(notes);
    }
    #endregion
    #region CashDiscount
    private void CashDiscount_OnClose()
    {
        refDiscountPopup.Hide();
    }
    private async Task CashDiscount_OnApply(decimal discountValue)
    {
        await _salesOrderViewModel.CalculateCashDiscount(discountValue);
        refDiscountPopup.Hide();
    }
    #endregion
    #region Save/Place Order
    private string PrepareSuccessMessage()
    {
        string message = string.Empty;
        switch (_salesOrderViewModel.Status)
        {
            case SalesOrderStatus.DELIVERED:
                message = @Localizer["order_delivered_successfully"];
                break;
            case SalesOrderStatus.DRAFT:
                message = @Localizer["order_saved_successfully"];
                break;
            default:
                break;
        }
        return message;
    }
    #endregion
    #region Validation
    private bool ValidateData()
    {
        bool isValid = true;

        // Reset any previous error messages
        ClearValidationError();

        if (_salesOrderViewModel.SelectedSalesOrderItemViews == null)
        {
            ValidationErrors.Add(@Localizer["sales_order_can_not_be_null"]);
            isValid = false;
        }
        // Item Count Validation
        else if (_salesOrderViewModel.SelectedSalesOrderItemViews != null
            && _salesOrderViewModel.SelectedSalesOrderItemViews.Count == 0)
        {
            ValidationErrors.Add(@Localizer["atleast_one_item_should_be_selected"]);
            isValid = false;
        }
        // NetAmount
        else if (_salesOrderViewModel.NetAmount <= 0)
        {
            ValidationErrors.Add(@Localizer["order_value_can_not_be_less_than_zero(0)"]);
            isValid = false;
        }
        return isValid;
    }
    #endregion

    private void HandleProductDelete(ISalesOrderItemView salesOrderItemView)
    {
        salesOrderItemView.Qty = 0;
        salesOrderItemView.IsCartItem = false;
        _salesOrderViewModel.OnQtyChange(salesOrderItemView);
        _salesOrderViewModel.DisplayedSalesOrderItemViews_Preview?.Remove(salesOrderItemView);
    }
}
