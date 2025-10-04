using Microsoft.AspNetCore.Components;
using Nest;
using Practice;
using System.Text;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Constants;
using Winit.Modules.Scheme.BL.Classes;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Store.BL.Classes;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Events;
using Winit.UIModels.Common;
using WinIt.Pages.ApprovalRoleEngine;

namespace WinIt.Pages.Scheme;

public partial class SelloutLiquidation
{
    [Parameter]
    public string SellOutHeaderUID { get; set; } = string.Empty;
    private bool IsEditMode => !string.IsNullOrEmpty(SellOutHeaderUID);
    private ISellOutSchemeLine SelectedProduct { get; set; }

    private bool ShowSerialNumberPopup { get; set; } = false;
    Winit.UIComponents.Web.DialogBox.ProductDialogBox<IPreviousOrders> AddProductDialogBox;
    private static Timer debounceTimer;
    private string SearchString = string.Empty;
    IEnumerable<ISellOutSchemeLine> Searcheditems
    {
        get
        {
            return string.IsNullOrEmpty(SearchString) ? _viewModel.SellOutMaster.SellOutSchemeLines :
                _viewModel.SellOutMaster.SellOutSchemeLines.Where(item => item.SkuName.Contains(SearchString));
        }
    }
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
        await _viewModel.PopulateViewModel();
        if (!_viewModel.IsNew)
        {
            await _viewModel.PopulateApprovalEngine(_viewModel.SellOutMaster.SellOutSchemeHeader!.UID);
        }
        HideLoader();
    }

    void OnSearch(string searchString)
    {

    }

    private async Task HandleChannelSelection(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
        {
            await _viewModel.OnChannelpartnerSelected(dropDownEvent);
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
            {
                _viewModel.PreviousOrdersList.AddRange(await ((SellOutSchemeWebViewModel)_viewModel).GetPreviousOrdersFromAPI(_viewModel.SelectedChannelPartner.UID));
            }
            else
            {
                _viewModel.PreviousOrdersList.Clear();
            }
            _viewModel.SellOutMaster.SellOutSchemeHeader.AvailableProvision2Amount = _viewModel.Branch_P2Amount;
            _viewModel.SellOutMaster.SellOutSchemeHeader.AvailableProvision3Amount = _viewModel.HO_P3Amount;
            _viewModel.SellOutMaster.SellOutSchemeHeader.StandingProvisionAmount = _viewModel.HO_S_Amount;
        }
        _viewModel.SellOutMaster.SellOutSchemeLines!.Clear();
        StateHasChanged();
    }
    private async Task HandleReasonSelection(DropDownEvent dropDownEvent, ISellOutSchemeLine sellOutSchemeLine)
    {
        if (dropDownEvent != null)
        {
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                sellOutSchemeLine.Reason = dropDownEvent.SelectionItems.FirstOrDefault()?.Code;
            else
                sellOutSchemeLine.Reason = string.Empty;
        }
        StateHasChanged();
    }
    private void OnContributionsEnter(ChangeEventArgs e, string contribution)
    {
        decimal val = CommonFunctions.RoundForSystem(e.Value ?? 0);
        switch (contribution)
        {
            case nameof(_viewModel.SellOutMaster.SellOutSchemeHeader.ContributionLevel1):
                _viewModel.SellOutMaster.SellOutSchemeHeader!.ContributionLevel1 = val;
                break;
            case nameof(_viewModel.SellOutMaster.SellOutSchemeHeader.ContributionLevel2):
                _viewModel.SellOutMaster.SellOutSchemeHeader!.ContributionLevel2 = val;
                break;
            case nameof(_viewModel.SellOutMaster.SellOutSchemeHeader.ContributionLevel3):
                _viewModel.SellOutMaster.SellOutSchemeHeader!.ContributionLevel3 = val;
                break;
        }
        StateHasChanged();
    }
    Winit.UIModels.Web.Breadcrum.Interfaces.IDataService dataService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
    {
        HeaderText = "Sell out Liquidation",
        BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>()
            {
                new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel(){SlNo=1,Text="Manage Scheme",IsClickable=true,URL="ManageScheme"},
                new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel(){SlNo=1,Text="Sell out liquidation",},
            }
    };



    public async Task<bool> Validate()
    {
        bool isValid = true;

        // Validate Channel Partner
        if (_viewModel.SelectedChannelPartner == null)
        {
            await _alertService.ShowErrorAlert("Alert", "Channel partner is required.");
            isValid = false;
            return isValid;
        }
        if (_viewModel.SellOutMaster.SellOutSchemeLines.Count == 0)
        {
            await _alertService.ShowErrorAlert("Alert", "Add atleast one item to continue.");
            isValid = false;
            return isValid = false;
        }
        List<string> qtyMsgs = [];
        List<string> reasonmsgs = [];
        List<string> crdntmsgs = [];
        _viewModel.SellOutMaster.SellOutSchemeLines.ForEach(p =>
        {
            string skuCode = p.SkuCode;
            if (p.Qty == 0)
            {
                qtyMsgs.Add(skuCode);
            }
            if (p.UnitCreditNoteAmount == 0)
            {
                crdntmsgs.Add(skuCode);
            }
            if (string.IsNullOrEmpty(p.Reason))
            {
                reasonmsgs.Add(skuCode);
            }
        });
        StringBuilder message = new StringBuilder();
        if (qtyMsgs.Count > 0)
        {
            message.Append($"Qty should'n be 0 for followng items : {string.Join(", ", qtyMsgs)} ...");
        }
        if (crdntmsgs.Count > 0)
        {
            message.Append($"Amount should'n be 0 for followng items : {string.Join(", ", crdntmsgs)} ...");
        }
        if (reasonmsgs.Count > 0)
        {
            message.Append($"Please select any one reason for followng items : {string.Join(", ", reasonmsgs)} ...");
        }
        if (!string.IsNullOrEmpty(message.ToString()))
        {
            await _alertService.ShowErrorAlert("Alert", message.ToString());
            isValid = false;
            return isValid = false;
        }
        //isValid = await _viewModel.ValidateContributions(_viewModel.SellOutMaster.SellOutSchemeHeader.ContributionLevel1, _viewModel.SellOutMaster.SellOutSchemeHeader.ContributionLevel2, _viewModel.SellOutMaster.SellOutSchemeHeader.ContributionLevel3);


        return isValid;
    }
    private void DeleteProduct(ISellOutSchemeLine product)
    {
        if (_viewModel.SellOutMaster.SellOutSchemeLines != null)
        {
            if (product.Id == 0)
            {
                _viewModel.SellOutMaster.SellOutSchemeLines.Remove(product);
                _alertService.ShowErrorAlert("Success", "Deleted succesfully");
            }
            else
            {
                product.IsDeleted = true;
                _alertService.ShowErrorAlert("Success", "Marked as deleted");
            }
            CalculateTotalCreditNoteAmount(product);
        }
    }

    private bool IsChannelPartnerSelected()
    {
        if (_viewModel.SelectedChannelPartner != null)
        {
            return true;
        }
        return false;
    }
    protected async Task OpenPreviousOrders()
    {
        if (_viewModel.SelectedChannelPartner == null)
        {
            await _alertService.ShowErrorAlert("Alert", "Select channel partner");
            return;
        }
        AddProductDialogBox?.OnOpenClick();
        await Task.CompletedTask;
    }

    public static void Debounce<T>(Action<T> action, T parameter, TimeSpan debounceDelay)
    {
        debounceTimer?.Dispose();
        debounceTimer = new Timer(state => action.Invoke(parameter), null, debounceDelay, Timeout.InfiniteTimeSpan);
    }
    private void DebouncedCalculateTotalCreditNoteAmountForQty(string qtyStr, ISellOutSchemeLine product)
    {
        if (int.TryParse(qtyStr, out int qty))
        {
            product.Qty = qty;
            CalculateTotalCreditNoteAmount(product);
        }
    }
    private void DebouncedCalculateTotalCreditNoteAmountForUnitCreditNoteAmount(string unitCreditNoteAmountStr, ISellOutSchemeLine product)
    {
        if (decimal.TryParse(unitCreditNoteAmountStr, out decimal unitCreditNoteAmount))
        {
            product.UnitCreditNoteAmount = unitCreditNoteAmount;
            CalculateTotalCreditNoteAmount(product);
        }
    }
    private void CalculateTotalCreditNoteAmount(ISellOutSchemeLine product)
    {
        product.TotalCreditNoteAmount = product.Qty * product.UnitCreditNoteAmount;
        _viewModel.SellOutMaster.SellOutSchemeHeader.TotalCreditNote = _viewModel.SellOutMaster.SellOutSchemeLines.Sum(p => p.TotalCreditNoteAmount);
        StateHasChanged();

    }
    private void CloseSerialNumberPopup()
    {
        if (SelectedProduct.SerialNos != null && SelectedProduct.SerialNos.Any())
        {
            SelectedProduct.SerialNos = string.Join(",", SelectedProduct.SerialNos);
        }
        ShowSerialNumberPopup = false;
        StateHasChanged();
    }
    int SlNoOfSerialNumbers = 0;
    private void ShowSerialNumberPopupm(ISellOutSchemeLine product)
    {
        if (product.SerialNosUILevel == null || product.SerialNosUILevel.Count == 0)
        {
            product.SerialNosUILevel = [];
            for (int i = 0; i < product.Qty; i++)
            {
                ISerialNumbers serial = _serviceProvider.CreateInstance<ISerialNumbers>();
                serial.SlNo = i + 1;
                product.SerialNosUILevel.Add(serial);
            }
        }
        SelectedProduct = product;
        ShowSerialNumberPopup = true;
        SlNoOfSerialNumbers = 0;
        StateHasChanged();
    }
    //protected void ClearAllFields()
    //{
    //    SelectedProduct = null;
    //    ShowSerialNumberPopup = false;
    //    if (_viewModel is not null)
    //    {
    //        _viewModel.SellOutMaster.SellOutSchemeLines.Clear();
    //        _viewModel.SellOutMaster.SellOutSchemeHeader.TotalCreditNote = 0;
    //        _viewModel.ChannelPartner.Clear();
    //        _viewModel.Branch_P2Amount = 0;
    //    }

    //    StateHasChanged();
    //}


    protected async Task SaveMasterData()
    {

        if (!await Validate())
        {
            return;
        }
        if (!await _alertService.ShowConfirmationReturnType("Confirm", IsEditMode ? "Are you sure you want to update?" : "Are you sure you want to save?"))
        {
            return;
        }
        if (_viewModel.IsNew)
        {
            _viewModel.SellOutMaster.SellOutSchemeHeader.Status = ApprovalConst.Pending;
        }
        bool result = await ((SellOutSchemeWebViewModel)_viewModel).SaveOrUpdate();


        if (result)
        {
            await _alertService.ShowErrorAlert("Success ", "Saved successfully");
            //ClearAllFields();
            if (_viewModel.IsNew)
            {
                //await executeRule(_iAppUser.Emp.Code, _viewModel.UserType, _viewModel.RequestId);
            }
            _navigationManager.NavigateTo("ManageScheme");
        }
        else
        {
            await _alertService.ShowErrorAlert("Error", "FAILED");
        }

    }
    #region ApprovalEngineLogic

    //public string UserCode { get; set; } = "423";//Usercode From Session
    //public string UserType { get; set; } = "Scheme";
    //public int RuleId { get; set; } = 11;
    //public int RequestId { get; set; }

    private async Task<ApprovalActionResponse> HandleApprovalAction(ApprovalStatusUpdate approvalStatusUpdate)
    {
        ApprovalActionResponse approvalActionResponse = new ApprovalActionResponse()
        {
            IsApprovalActionRequired = true
        };

        try
        {
            _viewModel.SellOutMaster.ApprovalStatusUpdate = approvalStatusUpdate;
            if (approvalStatusUpdate.IsFinalApproval)
            {
                if (approvalStatusUpdate.Status == ApprovalConst.Rejected)
                {
                    _viewModel.SellOutMaster.SellOutSchemeHeader.Status = ApprovalConst.Rejected;

                }
                else if (approvalStatusUpdate.Status == ApprovalConst.Approved)
                {
                    _viewModel.SellOutMaster.SellOutSchemeHeader.Status = SchemeConstants.Approved;
                }
            }
            else
            {
                if (approvalStatusUpdate.Status == ApprovalConst.Rejected)
                {
                    _viewModel.SellOutMaster.SellOutSchemeHeader.Status = SchemeConstants.Rejected;
                }
            }
            if (approvalStatusUpdate.Status != ApprovalConst.Reassign)
            {
                approvalActionResponse.IsSuccess = await ((SellOutSchemeWebViewModel)_viewModel).SaveOrUpdate();
                _navigationManager.NavigateTo("ManageScheme");
            }
        }
        catch (CustomException ex)
        {
            approvalActionResponse.IsSuccess = false;
        }
        return approvalActionResponse;
    }

    public async Task OnApprovalTracker(List<ApprovalStatusResponse> approvalStatusResponses) //After every action this method return all role
    {                                                                                        //with their status

    }

    #endregion
}

