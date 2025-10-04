using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Constants;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Common.Model.Constants.Notification;
using Winit.Modules.PurchaseOrder.BL.Events;
using Winit.Modules.PurchaseOrder.Model.Constatnts;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SMS.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using WinIt.Pages.ApprovalRoleEngine;

namespace WinIt.Pages.PurchaseOrder;

public partial class PurchaseOrderBasePage
{
    [Parameter]
    public string PurchaseOrderUID { get; set; } = string.Empty;
    private WinIt.Pages.DialogBoxes.AddProductDialogBoxV1<ISKUV1>? AddProductDialogBox;
    private bool IsDistributorSelect = false;
    private bool IsLoaded;
    private string ErrorMsg = string.Empty;
    private bool IsOrderPlaced = false;
    private bool IsFilterOpen = false;
    private bool IsApprovalEngineNeeded = false;
    private ElementReference alertErrorMsgElementRefference;
    public Dictionary<string, List<EmployeeDetail>>? ApprovalUserCodes { get; set; }
    private bool IsForApproval => _iAppUser.Role.IsPrincipalRole && !_viewModel.IsNewOrder;
    private readonly Winit.UIModels.Web.Breadcrum.Interfaces.IDataService _dataService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel
    {
        BreadcrumList =
        [
            new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel()
            {
                SlNo = 1,
                Text = "View Purchase Orders",
                IsClickable = true,
                URL = "ViewPurchaseOrderStatus"
            },
            new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel()
            {
                SlNo = 2,
                Text = "Purchase Order",
                IsClickable = false
            },
        ],
        HeaderText = "Purchase Order"
    };
    private readonly List<ISKUV1> SKUs = [];
    public List<IAllApprovalRequest> AllApprovalLevelList { get; set; } = [];
    private bool IsAuthorized = false;
    protected override async Task OnInitializedAsync()
    {
        ShowLoader();
        try
        {
            if (_appUser.Emp.UID == "")
            {

            }
            if (!_iAppUser.Role.IsDistributorRole && string.IsNullOrEmpty(PurchaseOrderUID))
            {
                await _viewModel.PrepareDistributors();
            }
            else
            {
                if (!string.IsNullOrEmpty(PurchaseOrderUID))
                {
                    _viewModel.IsNewOrder = true;
                    _viewModel.IsDraftOrder = false;
                    _viewModel.PurchaseOrderHeader.UID = PurchaseOrderUID;
                }

            }
            await InvokeAsync(async () =>
            {
                await _viewModel.PopulateViewModel(
                Winit.Shared.Models.Constants.SourceType.CPE,
                PurchaseOrderUID);
                if (_iAppUser.Role.IsDistributorRole || !string.IsNullOrEmpty(PurchaseOrderUID))
                {
                    await _viewModel.OnDistributorSelect();
                    AllApprovalLevelList = await _viewModel.GetAllApproveListDetails(PurchaseOrderUID);
                    if (AllApprovalLevelList != null && AllApprovalLevelList.Count > 0)
                    {
                        _viewModel.RequestId = int.Parse(AllApprovalLevelList[0]?.RequestID);
                        string? approvalUserDetail = AllApprovalLevelList[0]?.ApprovalUserDetail;

                        ApprovalUserCodes = string.IsNullOrEmpty(approvalUserDetail)
                        ? new Dictionary<string, List<EmployeeDetail>>()
                        : DeserializeApprovalUserCodes(approvalUserDetail) ?? new Dictionary<string, List<EmployeeDetail>>();



                    }
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
    private Dictionary<string, List<EmployeeDetail>> DeserializeApprovalUserCodes(string approvalUserDetail)
    {
        try
        {
            // First, attempt to deserialize assuming values are List<EmployeeDetail>
            return JsonConvert.DeserializeObject<Dictionary<string, List<EmployeeDetail>>>(approvalUserDetail);
        }
        catch (JsonSerializationException)
        {
            // If it fails, handle the case where values are List<string>
            var stringListDictionary = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(approvalUserDetail);

            if (stringListDictionary != null)
            {
                // Convert List<string> to List<EmployeeDetail>
                return stringListDictionary.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(code => new EmployeeDetail { EmpCode = code, EmpName = code }).ToList()
                );
            }

            // If all deserialization attempts fail, return null
            return null;
        }
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
        else if (_viewModel.PurchaseOrderHeader.Status is PurchaseOrderStatusConst.InProcessERP or
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

    private async Task OnAddProductClick(List<ISKUV1> sKUs)
    {

        try
        {
            ShowLoader();
            List<string> skus = sKUs.Select(e => e.UID).Distinct().ToList();
            if (skus == null || !skus.Any())
            {
                throw new CustomException(ExceptionStatus.Failed, "Please select items..");
            }

            await _viewModel.AddProductsToGridBySKUUIDs(skus);
        }
        catch (CustomException ex)
        {
            if (ex.Status == Winit.Shared.Models.Enums.ExceptionStatus.Success)
            {
                ShowSuccessSnackBar("Failed", ex.Message);
            }
            else
            {
                ShowErrorSnackBar("Success", ex.Message);
            }
        }
        HideLoader();
    }
    private async Task<List<ISelectionItem>> OnDropdownValueSelectSKUAtrributes(ISelectionItem selectedValue)
    {
        return await _viewModel.OnSKuAttributeDropdownValueSelect(selectedValue.UID);
    }

    private async Task OnQtyChange(PurchaseOrderItemEvent purchaseOrderItemEvent)
    {
        if (!IsForApproval)
        {
            purchaseOrderItemEvent.PurchaseOrderItemView.RequestedQty = purchaseOrderItemEvent.Qty;
        }
        purchaseOrderItemEvent.PurchaseOrderItemView.FinalQty = purchaseOrderItemEvent.Qty;
        await _viewModel.OnQtyChange(purchaseOrderItemEvent.PurchaseOrderItemView);
    }

    private async Task OnTemlateSelect(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent == null || dropDownEvent.SelectionItems == null || !dropDownEvent.SelectionItems.Any())
        {
            await _viewModel.ClearTemplateItems();

            return;
        }
        if (string.IsNullOrEmpty(_viewModel.PurchaseOrderHeader.ShippingAddressUID))
        {
            dropDownEvent.SelectionItems.First().IsSelected = false;
            ShowErrorSnackBar("Alert", "Please Select ShipTo Address.");
        }
        try
        {

            ShowLoader();
            await _viewModel.OnTemplateSelect(dropDownEvent.SelectionItems.FirstOrDefault()!.UID);
        }
        catch (Exception)
        {
        }
        HideLoader();
    }

    private async Task OnOrgUnitSelect(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent == null)
        {
            return;
        }
        if (dropDownEvent.SelectionItems == null || !dropDownEvent.SelectionItems.Any())
        {
            _viewModel.ClearOrgUnitSelection();
            return;
        }
        try
        {
            if (_viewModel.IsDraftOrder/*_viewModel.PurchaseOrderItemViews.Any() &&*/
                )
            {
                if (_viewModel.PurchaseOrderHeader.OrgUnitUID != dropDownEvent.SelectionItems.First()?.UID && await _alertService.ShowConfirmationReturnType("Alert",
                "Are you sure you want to change the Organization Unit? Changing it will delete all added items."))
                {
                    ShowLoader();
                    _viewModel.ClearOrgUnitSelection();
                    await _viewModel.OnOrgUnitSelect(dropDownEvent.SelectionItems.First().UID, true);
                }
                else
                {
                    ShowLoader();
                    await _viewModel.OnOrgUnitSelect(dropDownEvent.SelectionItems.First().UID, false);
                }
            }
            else if (!_viewModel.IsDraftOrder)
            {
                ShowLoader();
                _viewModel.ClearOrgUnitSelection();
                await _viewModel.OnOrgUnitSelect(dropDownEvent.SelectionItems.First().UID);
            }
            else
            {
                dropDownEvent.SelectionItems.ForEach(e => e.IsSelected = false);
            }
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
        finally
        {
            HideLoader();
        }

    }

    private void OnWareHouseSelect(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent == null)
        {
            return;
        }
        if (dropDownEvent.SelectionItems == null || !dropDownEvent.SelectionItems.Any())
        {
            _viewModel.PurchaseOrderHeader.WareHouseUID = string.Empty;
            return;
        }

        _viewModel.PurchaseOrderHeader.WareHouseUID = dropDownEvent.SelectionItems.FirstOrDefault()!.UID;
    }
    private async Task OnShippingAddressSelect(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent == null)
        {
            return;
        }

        if (dropDownEvent.SelectionItems == null || !dropDownEvent.SelectionItems.Any())
        {
            _viewModel.ClearShippingAddressSelection();
            return;
        }
        try
        {
            ShowLoader();
            _viewModel.ClearShippingAddressSelection();
            _viewModel.PurchaseOrderHeader.ShippingAddressUID = dropDownEvent.SelectionItems.FirstOrDefault()!.UID;
            await _viewModel.OnShipToAddressSelect(_viewModel.PurchaseOrderHeader.ShippingAddressUID);
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
        finally
        {
            HideLoader();
        }
    }
    private void OnBillingAddressSelect(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent == null)
        {
            return;
        }
        if (dropDownEvent.SelectionItems == null || !dropDownEvent.SelectionItems.Any())
        {
            _viewModel.PurchaseOrderHeader.BillingAddressUID = string.Empty;
            return;
        }

        _viewModel.PurchaseOrderHeader.BillingAddressUID = dropDownEvent.SelectionItems.FirstOrDefault()!.UID;
    }

    private async Task OnSaveAsDraftClick()
    {
        try
        {
            ShowLoader();
            _viewModel.Validate();
            await _viewModel.SaveOrder();
        }
        catch (CustomException ex)
        {
            if (ex.Status == Winit.Shared.Models.Enums.ExceptionStatus.Success)
            {
                ShowSuccessSnackBar("Purchase Order", ex.Message);
                _navigationManager.NavigateTo("ViewPurchaseOrderStatus");
            }
            else
            {
                ShowErrorSnackBar("Purchase Order", ex.Message);
            }
        }
        finally { HideLoader(); }
    }
    private async Task OnConfirmOrderClick()
    {
        try
        {
            if (await _alertService.ShowConfirmationReturnType("Confirm", "Are you sure you want to place this order?"))
            {
                ShowLoader();
                _viewModel.Validate();
                _viewModel.PurchaseOrderHeader.CreatedTime = DateTime.Now;
                _viewModel.PurchaseOrderHeader.OrderDate = DateTime.Now;
                await _viewModel.SaveOrder(PurchaseOrderStatusConst.PendingForApproval);
            }
        }
        catch (CustomException ex)
        {
            if (ex.Status == Winit.Shared.Models.Enums.ExceptionStatus.Success)
            {
                IsOrderPlaced = true;
                List<string> smsTemplates = new List<string>();
                if (_viewModel.IsPoEdited || _viewModel.IsPoCreatedByCP)
                {
                    smsTemplates = new List<string>
                    {
                        NotificationTemplateNames.PO_CREATED_BY_CP_SEND_TO_ASM_FOR_APPROVAL,
                        NotificationTemplateNames.PO_CREATED_BY_CP_SEND_TO_BM_FOR_INFO,
                    };
                }
                else
                {
                    smsTemplates = new List<string>
                    {
                        NotificationTemplateNames.PO_APPROVED_BY_CP_SEND_TO_BM_FOR_APPROVAL,
                    };
                }

                _ = _viewModel.SendEmail(smsTemplates);
                _ = _viewModel.SendSms(smsTemplates);
            }
            else
            {
                ShowErrorSnackBar("Purchase Order", ex.Message);
            }
        }
        finally
        {
            HideLoader();
        }
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
    private async Task OnDeleteSelectedItems()
    {
        try
        {
            List<string> selectedSKUUIDs = _viewModel.PurchaseOrderItemViews.Where(s => s.IsSelected).Select(e => e.SKUUID).ToList();
            if (selectedSKUUIDs.Any() && await _alertService.ShowConfirmationReturnType("Alert", "Are you sure you want to delete selected items?"))
            {
                ShowLoader();
                _ = SKUs.RemoveAll(e => selectedSKUUIDs.Contains(e.UID));

                await InvokeAsync(async () => await _viewModel.DeleteSelectedItems());
            }
            else
            {
                throw new CustomException(ExceptionStatus.Failed, "Please select items to delete");
            }
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
        HideLoader();
    }

    //------------------------------------Approval Engine Code------------------------

    private string? ErrorMessage { get; set; }
    private ApprovalEngine? childComponentRef;


    /// <summary>
    /// Used handel after all approvals
    /// </summary>
    /// <returns></returns>
    private async Task<ApprovalActionResponse> HandelApprovalClick(ApprovalStatusUpdate approvalStatusUpdate)
    {
        ApprovalActionResponse approvalActionResponse = new ApprovalActionResponse();
        approvalActionResponse.IsApprovalActionRequired = true;
        approvalActionResponse.IsSuccess = true;
        try
        {
            bool isCreditLimitValid = await ValidateCreditLimit();
            _viewModel.ApprovalStatusUpdate = approvalStatusUpdate;
            if (approvalStatusUpdate.IsFinalApproval && isCreditLimitValid)
            {
                _viewModel.ApplyApprovedQty();
                await _viewModel.SaveOrder(PurchaseOrderStatusConst.InProcessERP);
                return approvalActionResponse;
            }
            else if (approvalStatusUpdate.Status == ApprovalConst.Rejected)
            {
                await _viewModel.SaveOrder(PurchaseOrderStatusConst.CancelledByCMI);
                return approvalActionResponse;
            }
            else if (isCreditLimitValid)
            {
                _viewModel.ApplyApprovedQty();
                await _viewModel.SaveOrder(PurchaseOrderStatusConst.PendingForApproval);
                return approvalActionResponse;
            }
            
            StateHasChanged();
            approvalActionResponse.IsSuccess = false;
            return approvalActionResponse;
        }
        catch (CustomException ex)
        {
            if (approvalStatusUpdate.IsFinalApproval)
            {
                List<string> smsTemplates = new List<string>
                {
                    NotificationTemplateNames.PO_APPROVED_BY_BM_SEND_TO_ASM_FOR_INFO,
                    NotificationTemplateNames.PO_APPROVED_BY_LAST_LEVEL_SEND_TO_CP_FOR_INFO,
                };
                _ = _viewModel.SendEmail(smsTemplates);
                _ = _viewModel.SendSms(smsTemplates);
            }
            //if (approvalStatusUpdate.RoleCode == PurchaseOrderStatusConst.ASM)
            //{
            //    List<string> smsTemplates = new List<string>
            //    {
            //        NotificationTemplateNames.PO_APPROVED_BY_CP_SEND_TO_BM_FOR_APPROVAL,
            //    };
            //    _ = _viewModel.SendEmail(smsTemplates);
            //    _ = _viewModel.SendSms(smsTemplates);
            //}
            if (ex.Status == Winit.Shared.Models.Enums.ExceptionStatus.Success)
            {
                if (_viewModel.PurchaseOrderHeader.Status == PurchaseOrderStatusConst.InProcessERP)
                {
                    await _viewModel.InsertDataInIntegrationDB();
                }
                return approvalActionResponse;
            }
            else
            {
                ShowErrorSnackBar("Failed", ex.Message);
                approvalActionResponse.IsSuccess = false;
                StateHasChanged();
                return approvalActionResponse;
            }
        }
    }

    private async Task<bool> ValidateCreditLimit()
    {
        try
        {
            await _viewModel.ValidateCreditLimit();
            return true;
        }
        catch (CustomException ex)
        {
            if (ex.Status == Winit.Shared.Models.Enums.ExceptionStatus.Success)
            {
                //ShowSuccessSnackBar("Alert", ex.Message);
                return true;
            }
            else
            {
                ErrorMsg = ex.Message;
                alertErrorMsgElementRefference.FocusAsync();
                throw new CustomException(ExceptionStatus.Failed, "Approval failed please check the Error Message");
            }
        }
        catch (Exception e)
        {
            throw;
        }
    }

    private async Task OnCreateApprovalClick()
    {
        try
        {
            ShowLoader();
            await _viewModel.CreateApproval();
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
        catch (Exception ex)
        {
            throw;
        }
        finally
        {
            HideLoader();
        }
    }

    private bool OnItemSelect(ISKUV1 item)
    {
        if (!_appSetting.IsAPMasterValidationRequired) return true;
        if (!item.IsAvailableInApMaster)
        {
            ShowErrorSnackBar("Alert", "This model is not part of AP master please contact CMI team ");
        }
        return item.IsAvailableInApMaster;
    }
}
