using Microsoft.AspNetCore.Components;
using Winit.Modules.PurchaseOrder.Model.Constatnts;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using WinIt.Pages.Base;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace WinIt.Pages.POReturnOrder;

public partial class POReturnOrderBasePage : BaseComponentBase
{
    [Parameter]
    public string ReturnOrderUID { get; set; } = string.Empty;
    private WinIt.Pages.DialogBoxes.AddProductDialogBoxV1<ISKUV1>? AddProductDialogBox;
    private bool IsDistributorSelect = false;
    private bool IsLoaded;
    private string ErrorMsg = string.Empty;
    private bool IsOrderPlaced = false;
    private bool IsFilterOpen = false;
    private bool IsApprovalEngineNeeded = false;
    private bool IsForApproval => _iAppUser.Role.IsPrincipalRole && !_viewModel.IsNewOrder;
    public bool IsInvoiceSelect { get; set; }
    private bool IsAuthorized = false;
    private readonly Winit.UIModels.Web.Breadcrum.Interfaces.IDataService _dataService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel
    {
        BreadcrumList =
        [
            new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel()
            {
                SlNo = 1,
                Text = "View Return Orders",
                IsClickable = true,
                URL = ""
            },
            new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel()
            {
                SlNo = 2,
                Text = "Return Order",
                IsClickable = false
            },
        ],
        HeaderText = "Return Order"
    };


    protected override async Task OnInitializedAsync()
    {
        ShowLoader();
        try
        {
            if (!_iAppUser.Role.IsDistributorRole && string.IsNullOrEmpty(ReturnOrderUID))
            {
                await _viewModel.PrepareDistributors();
            }
            else
            {
                if (!string.IsNullOrEmpty(ReturnOrderUID))
                {
                    _viewModel.IsNewOrder = true;
                    _viewModel.IsDraftOrder = false;
                    _viewModel.ReturnOrder.UID = ReturnOrderUID;
                }

            }
            await InvokeAsync(async () =>
            {
                await _viewModel.PopulateViewModel(
                Winit.Shared.Models.Constants.SourceType.CPE,
                ReturnOrderUID);
                if (_iAppUser.Role.IsDistributorRole || !string.IsNullOrEmpty(ReturnOrderUID))
                {
                    await _viewModel.OnDistributorSelect();

                }
            });
            await ValidatePageAccess();

            StateHasChanged();
        }
        catch (CustomException ex)
        {
            if (ex.Status == Winit.Shared.Models.Enums.ExceptionStatus.Success)
            {
                ShowSuccessSnackBar("Success", ex.Message);
            }
            else
            {
                ShowErrorSnackBar("Error", ex.Message);
            }
        }
        finally
        {
            SetHeaderText();
            IsLoaded = true;
            IsApprovalEngineNeeded = true;
            //_viewModel.GenerateApprovalRequestIdAndRuleId();
            HideLoader();
        }
    }
    private void SetHeaderText()
    {
        if (IsForApproval)
        {
            _dataService.HeaderText = "Approve";
        }
        else if (_viewModel.IsNewOrder)
        {
            _dataService.HeaderText = "Create";
        }
        else if (_viewModel.ReturnOrder.Status is PurchaseOrderStatusConst.InProcessERP or
            PurchaseOrderStatusConst.Invoiced)
        {
            _dataService.HeaderText = "View";
        }
        else
        {
            _dataService.HeaderText += string.Empty;
        }
        _dataService.HeaderText += " Purchase Order";

    }

    private async Task OnDistributorSelect()
    {
        try
        {
            if (_viewModel.SelectedDistributor == null)
            {
                ShowErrorSnackBar("Failed", "Please Select Distributor...");
                return;
            }
            if (_viewModel.SelectedStoreMaster != null && _viewModel.SelectedDistributor == _viewModel.SelectedStoreMaster.Store.UID)
            {
                return;
            }
            IsDistributorSelect = false;
            IStore selectedStore = _viewModel.FilteredStores.ToList().Find(e => e.UID == _viewModel.SelectedDistributor);
            if (selectedStore.IsBlocked)
            {
                _alertService.ShowErrorAlert("Alert", $"This customer is blocked due to reason:  {selectedStore.BlockedReasonDescription ?? "N/A"} ");
                return;
            }
            //IsDistributorSelectPopUp = false;
            await InvokeAsync(async () =>
            {
                ShowLoader();
                await _viewModel.OnDistributorSelect();
            });
            StateHasChanged();
        }
        catch (CustomException ex)
        {
            if (ex.Status == Winit.Shared.Models.Enums.ExceptionStatus.Success)
            {
                ShowSuccessSnackBar("Success", ex.Message);
            }
            else
            {
                ShowErrorSnackBar("Failed", ex.Message);
            }
        }
        HideLoader();
    }
    private async Task ValidatePageAccess()
    {
        AuthenticationState authState = await _authStateProvider.GetAuthenticationStateAsync();
        System.Security.Claims.ClaimsPrincipal user = authState.User;
        if (!_viewModel.IsNewOrder)
        {
            IsAuthorized = true;
        }
        else if (_viewModel.IsNewOrder &&
            user.Identity?.IsAuthenticated == true &&
            (user.IsInRole("ASM") || user.IsInRole("Distributor")))
        {
            IsAuthorized = true;
        }
        else
        {
            IsAuthorized = false;
            throw new CustomException(ExceptionStatus.Failed, "Not authorized");
        }
    }

    public async Task OnInvoiceSelect(string invoiceUID)
    {
        
    }

}
