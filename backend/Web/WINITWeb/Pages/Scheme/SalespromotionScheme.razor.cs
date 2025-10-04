using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using Winit.Shared.Models.Events;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.UIModels.Common;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.BL.Classes;
using Winit.Modules.ApprovalEngine.Model.Constants;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Shared.Models.Common;

namespace WinIt.Pages.Scheme;

public partial class SalespromotionScheme
{
    [Parameter]
    public string UID { get; set; }

    public Winit.UIComponents.Common.FileUploader.FileUploader? fileUploader;
    public Winit.UIComponents.Common.FileUploader.FileUploader? fileUploaderApproveMode;
    string FilePath = string.Empty;
    Winit.UIModels.Web.Breadcrum.Interfaces.IDataService dataService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
    {
        HeaderText = "Sales Promotion",
        BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>()
        {
             new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel(){SlNo=1,Text="Manage Scheme",URL="ManageScheme",IsClickable=true},
         }
    };
    List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> ModifiedApprovedFiles { get; set; } = [];
    SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO = new SalesPromotionSchemeApprovalDTO();
    protected override void OnInitialized()
    {
        _viewModel.GetUserTypeWhileCreatingScheme(true, out string userType, out int ruleId);
        _viewModel.RuleId = ruleId;
        _viewModel.UserType = userType;

        base.OnInitialized();
    }
    protected override async Task OnInitializedAsync()
    {
        ShowLoader();
        try
        {
            await _viewModel.PopulateViewModel();
            if (_viewModel.IsNew)
            {
                //await _viewModel.PopulateViewModel();
                dataService.BreadcrumList.Add(new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel()
                {
                    SlNo = 2,
                    Text = dataService.HeaderText,
                });
            }
            else
            {
                dataService.BreadcrumList.Add(new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel()
                {
                    SlNo = 2,
                    Text = dataService.HeaderText = "Create new sales promotion",
                });

            }

            FilePath = FileSysTemplateControles.GetSchemeeFolderPath(_viewModel.PromotionUID);
        }
        catch (Exception ex)
        {

        }
        finally
        {
            _viewModel.IsIntialize = true;
            HideLoader();
        }
        await base.OnInitializedAsync();
    }
    private async Task ValidateContributions()
    {
        await _viewModel.ValidateContributions(_viewModel.SalesPromotion.ContributionLevel1,
            _viewModel.SalesPromotion.ContributionLevel2, _viewModel.SalesPromotion.ContributionLevel3);
    }
    private async Task<bool> ValidateForm()
    {
        bool isValid = true;

        // Validate Channel Partner
        if (_viewModel.SelectedChannelPartner == null)
        {
            await _alertService.ShowErrorAlert("Alert", "Channel partner is required.");
            isValid = false;
            return isValid;
        }
        //if (!_viewModel.SalesPromotion.IsPOHandledByDMS)
        //    isValid = await _viewModel.ValidateContributions(_viewModel.SalesPromotion.ContributionLevel1,
        //        _viewModel.SalesPromotion.ContributionLevel2, _viewModel.SalesPromotion.ContributionLevel3);
        //if (!isValid)
        //{
        //    return isValid;
        //}
        if (string.IsNullOrWhiteSpace(_viewModel.SalesPromotion.ActivityType))
        {
            await _alertService.ShowErrorAlert("Alert", "Activity type is required.");
            isValid = false;
            return isValid;
        }
        if (_viewModel.SalesPromotion.FromDate == default || _viewModel.SalesPromotion.ToDate == default)
        {
            await _alertService.ShowErrorAlert("Alert", "From date and to-date are required.");
            isValid = false;
            return isValid;
        }
        else if (_viewModel.SalesPromotion.FromDate >= _viewModel.SalesPromotion.ToDate)
        {
            await _alertService.ShowErrorAlert("Alert", "To date must be after from date.");
            isValid = false;
            return isValid;
        }
        if (_viewModel.SalesPromotion.Amount == null || _viewModel.SalesPromotion.Amount <= 0)
        {
            await _alertService.ShowErrorAlert("Alert", "Amount must be greater than zero.");
            isValid = false;
            return isValid;
        }
        if (string.IsNullOrWhiteSpace(_viewModel.SalesPromotion.Description))
        {
            await _alertService.ShowErrorAlert("Alert", "Description type is required.");
            isValid = false;
            return isValid;
        }

        return isValid;
    }

    private void ClearFields()
    {
        _viewModel.SalesPromotion = new SalesPromotionScheme();

        _viewModel.SelectedChannelPartner = null;

        StateHasChanged();
    }
    private async Task HandleSubmit()
    {
        ShowLoader();
        if (await ValidateForm())
        {
            bool res = false;

            if (_viewModel.IsNew)
            {
                res = await _viewModel.CreateSalesPromotion();
                if (res)
                {
                    // await executeRule(_iAppUser.Emp.Code, _viewModel.UserType, _viewModel.RequestId);
                }
            }
            else
            {
                SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO = new SalesPromotionSchemeApprovalDTO();
                salesPromotionSchemeApprovalDTO.SalesPromotion = _viewModel.SalesPromotion;
                salesPromotionSchemeApprovalDTO.SalesPromotion.Status = SchemeConstants.Pending;
                res = await _viewModel.UpdateSalesPromotion(salesPromotionSchemeApprovalDTO);
            }
            if (res)
            {
                if (await SaveFiles())
                {
                    ClearFields();
                    _navigationManager.NavigateTo("ManageScheme");
                }
            }

        }
        HideLoader(); 

    }
    private async Task<bool> SaveFiles()
    {
        if (_viewModel.ModifiedFileSysList.Count > 0)
        {
            var response = await fileUploader?.MoveFiles();
            if (response != null && response.IsSuccess)
            {
                response = await _viewModel.UploadFiles(_viewModel.ModifiedFileSysList);
                if (response == null || !response.IsSuccess)
                {
                    _tost.Add("Error", response?.ErrorMessage ?? "Error while uploading files", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                    return false;
                }
                else
                {
                    _viewModel.ModifiedFileSysList.Clear();
                }
            }
        }
        return true;
    }



    private async Task HandleChannelSelection(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
        {
            await _viewModel.OnChannelpartnerSelected(dropDownEvent);

            _viewModel.SalesPromotion.AvailableProvision2Amount = _viewModel.Branch_P2Amount;
            _viewModel.SalesPromotion.AvailableProvision3Amount = _viewModel.HO_P3Amount;
            _viewModel.SalesPromotion.StandingProvisionAmount = _viewModel.HO_S_Amount;
        }
        StateHasChanged();
    }

    protected async Task<bool> RejectSalesPromotion(SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO)
    {
        if (!_viewModel.IsNew)
        {
            bool res = await _viewModel.RejectSalesPromotion(salesPromotionSchemeApprovalDTO);
            if (res)
            {
                _navigationManager.NavigateTo("ManageScheme");
                return true;
            }
            else
            {
                _tost.Add("Failed", "Rejection failed");
                return false;
            }
        }
        return default;
    }
    async Task<bool> ValidatePOSlate()
    {
        bool isVal = true;
        string message = string.Empty;
        if (string.IsNullOrEmpty(_viewModel.SalesPromotion.PoNumber))
        {
            message += "PO Number created in oracle ";
            isVal = false;
        }
        if (!string.IsNullOrEmpty(message))
        {
            message = message + ",";
        }
        if (_viewModel.SalesPromotion.PoDate == null)
        {
            message += " Date";
            isVal = false;
        }
        if (!isVal)
        {
            await _alertService.ShowErrorAlert("Error", $"Following field(s) shouldn't be empty :{message}");
        }
        return isVal;
    }
    protected async Task<bool> UpdateSalesPromotion(SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO)
    {
        bool isValidated = await ValidatePOSlate();
        if (!isValidated)
        {
            return false;
        }
        bool res = await _viewModel.UpdateSalesPromotion(salesPromotionSchemeApprovalDTO);
        if (res)
        {
            await SaveFiles();
            base.StateHasChanged();
            //_tost.Add("Success", "Aprrove Successfully");
            _navigationManager.NavigateTo(SchemeConstants.Approved.Equals(salesPromotionSchemeApprovalDTO.SalesPromotion.Status, StringComparison.OrdinalIgnoreCase) ? "executepromotion" : "ManageScheme");
        }
        else
        {
            _tost.Add("Failed", "Aprrove failed");
        }
        return res;
    }

    private async Task<ApprovalActionResponse> HandleApprovalAction(ApprovalStatusUpdate approvalStatusUpdate)
    {
        ApprovalActionResponse approvalActionResponse = new ApprovalActionResponse()
        {
            IsApprovalActionRequired = true,
        };

        try
        {
            salesPromotionSchemeApprovalDTO.SalesPromotion = _viewModel.SalesPromotion;
            salesPromotionSchemeApprovalDTO.ApprovalStatusUpdate = approvalStatusUpdate;
            if (approvalStatusUpdate.Status == ApprovalConst.Rejected)
            {
                salesPromotionSchemeApprovalDTO.SalesPromotion.Status = ApprovalConst.Rejected;
                approvalActionResponse.IsSuccess = await UpdateSalesPromotion(salesPromotionSchemeApprovalDTO);
                return approvalActionResponse;
            }
            else if (approvalStatusUpdate.IsFinalApproval)
            {
                salesPromotionSchemeApprovalDTO.SalesPromotion.Status = ApprovalConst.Approved;
                approvalActionResponse.IsSuccess = await UpdateSalesPromotion(salesPromotionSchemeApprovalDTO);
                return approvalActionResponse;
            }
            else if (approvalStatusUpdate.Status != ApprovalConst.Reassign)
            {
                approvalActionResponse.IsSuccess = await UpdateSalesPromotion(salesPromotionSchemeApprovalDTO);
            }
            return approvalActionResponse;
        }
        catch (CustomException ex)
        {
            //if (ex.Status == Winit.Shared.Models.Enums.ExceptionStatus.Success)
            //{
            //    //ShowSuccessSnackBar("Success", "Approval Success");
            //    //if (_viewModel.PurchaseOrderHeader.Status == PurchaseOrderStatusConst.InProcessERP)
            //    //{
            //    //    await _viewModel.InsertDataInIntegrationDB();
            //    //}
            //    return true;
            //}
            //else
            //{
            //    ShowErrorSnackBar("Failed", ex.Message);
            approvalActionResponse.IsSuccess = false;
            return approvalActionResponse;
            //}
        }
    }
    //private async Task HandleApproved()
    //{
    //    //after all level of approvals
    //    // await UpdateSalesPromotion(SchemeConstants.Approved);
    //}
    bool isLoad { get; set; }
    //protected override async Task HandleExecuteRule(ApprovalApiResponse<ApprovalStatus> res)
    //{
    //    if (res.Action == ApprovalConst.Approved)
    //    {

    //    }
    //}

    public async Task OnApprovalTracker(List<ApprovalStatusResponse> approvalStatusResponses)
    {
        if (isLoad)
        {
            //await UpdateSalesPromotion(SchemeConstants.Pending);
        }
        else
        {
            //_viewModel.IsApproveMode = approvalStatusResponses.Any(p => SchemeConstants.Pending.Equals(p.Status));
            if (approvalStatusResponses is null || approvalStatusResponses.Count == 0)
            {
                return;
            }
            bool hasAccess = approvalStatusResponses.Any(p => p.ApproverId == _iAppUser.Role.Code);
            if (hasAccess)
            {
                _viewModel.ShowPOTab = true;
            }
            isLoad = true;
        }
    }
    //private async Task<bool> OnApproveClick(bool isFinalArroval)
    //{
    //    bool isValidated = await UpdateSalesPromotion(isFinalArroval ? SchemeConstants.Approved : SchemeConstants.Pending); ;
    //    if (isValidated)
    //    {
    //        _navigationManager.NavigateTo(isFinalArroval ? "executepromotion" : "ManageScheme");
    //    }
    //    return isValidated;
    //}
}
