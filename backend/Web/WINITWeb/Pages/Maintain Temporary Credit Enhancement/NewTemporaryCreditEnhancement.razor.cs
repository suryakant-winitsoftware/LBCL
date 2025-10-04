using Microsoft.AspNetCore.Components;
using Nest;
using Winit.Modules.CreditLimit.Model.Classes;
using Winit.Modules.CreditLimit.Model.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.FileUploader;
using Winit.UIComponents.Common.WinitDatepicker;
using Winit.UIModels.Common;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using WinIt.Pages.Base;
using Winit.Shared.CommonUtilities.Common;

namespace WinIt.Pages.Maintain_Temporary_Credit_Enhancement;

public partial class NewTemporaryCreditEnhancement : BaseComponentBase
{
    private Winit.UIComponents.Common.FileUploader.FileUploader? fileUploader { get; set; }
    private string? FilePath { get; set; }
    private string SelectedRequestType = string.Empty;
    private string CPUIDForCredit = string.Empty;
    private string DivUIDForCredit = string.Empty;
    public string? ValidationError;
    public string? TemporaryCreditEnhancementUID { get; set; }
    public bool IsInitialised { get; set; } = false;
    public bool IsView { get; set; } = false;
    private List<DataGridColumn> DataGridColumns = new List<DataGridColumn>();
    IDataService dataService = new DataServiceModel()
    {
        HeaderText = "Add Temporary Credit Enhancement",
        BreadcrumList = new List<IBreadCrum>()
        {
            new BreadCrumModel()
            {
                SlNo = 1,
                Text = "Temporary Credit Enhancement",
                URL = "TemporaryCreditEnhancement",
                IsClickable = true
            },
            new BreadCrumModel()
            {
                SlNo = 1, 
                Text = "Add Temporary Credit Enhancement"
            },
        }
    };
    protected override async Task OnInitializedAsync()
    {
        ShowLoader();
        FilePath = FileSysTemplateControles.GetOnBoardImageCheckFolderPath(_viewModel.TemporaryCreditEnhancementDetails.UID);
        GenerateGridColumns();
        TemporaryCreditEnhancementUID = _commonFunctions.GetParameterValueFromURL("CreditEnhancementUID");
        await _viewModel.PopulateChannelPartners();

        if (TemporaryCreditEnhancementUID != null)
        {
            await _viewModel.GetTemporaryCreditRequestDetailsByUID(TemporaryCreditEnhancementUID);
            await _viewModel.PopulateDivisionSelectionList(_viewModel.TemporaryCreditEnhancementDetails.StoreUID);
            PopulatePageWithData();
            IsView = true;
        }
        else
        {
            await _viewModel.PopulateViewModel();
        }
        IsInitialised = true;
        HideLoader();
    }

    private void PopulatePageWithData()
    {
        foreach (var partner in _viewModel.ChannelPartnerList)
        {
            if (partner.UID == _viewModel.TemporaryCreditEnhancementDetails.StoreUID)
            {
                partner.IsSelected = true;
                break;
            }
        }
        foreach (var partner in _viewModel.DivisionsList)
        {
            if (partner.UID == _viewModel.TemporaryCreditEnhancementDetails.DivisionOrgUID)
            {
                partner.IsSelected = true;
                break;
            }
        }
        foreach (var partner in _viewModel.TemporaryCreditEnhancementRequestselectionItems)
        {
            if (partner.Label == _viewModel.TemporaryCreditEnhancementDetails.RequestType)
            {
                partner.IsSelected = true;
                SelectedRequestType = partner.Label;
                break;
            }
        }
    }

    public void OnChannelPartnerSelect(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent.SelectionItems.Any(item => item.IsSelected))
        {
            if (string.IsNullOrEmpty(SelectedRequestType))
            {
                var selectedItem = dropDownEvent.SelectionItems.FirstOrDefault(item => item.IsSelected);
                _viewModel.TemporaryCreditEnhancementDetails.StoreUID = selectedItem.UID;
                _viewModel.PopulateDivisionSelectionList(selectedItem.UID);
                CPUIDForCredit = selectedItem.UID;
                ValidateDropDownsToGetCreditLimits();
            }
            else
            {
                SelectedRequestType = string.Empty;
                _viewModel.TemporaryCreditEnhancementRequestselectionItems
                    .ForEach(item => item.IsSelected = false);
                StateHasChanged();
            }
        }
        else
        {
            _viewModel.TemporaryCreditEnhancementRequestselectionItems
                .ForEach(item => item.IsSelected = false);
            SelectedRequestType = string.Empty;
            CPUIDForCredit = string.Empty;
            ValidateDropDownsToGetCreditLimits();
            StateHasChanged();
        }
    }
    public void OnDivisionSelect(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent.SelectionItems.Any(item => item.IsSelected))
        {
            if (string.IsNullOrEmpty(SelectedRequestType))
            {
                var selectedItem = dropDownEvent.SelectionItems.FirstOrDefault(item => item.IsSelected);
                _viewModel.TemporaryCreditEnhancementDetails.DivisionOrgUID = selectedItem.UID;
                DivUIDForCredit = selectedItem.UID;
                //ValidateDropDownsToGetCreditLimits();
            }
            else
            {
                _viewModel.TemporaryCreditEnhancementRequestselectionItems
                    .ForEach(item => item.IsSelected = false);
                SelectedRequestType = string.Empty;
                StateHasChanged();
            }
        }
        else
        {
            _viewModel.TemporaryCreditEnhancementRequestselectionItems
                .ForEach(item => item.IsSelected = false);
            DivUIDForCredit = string.Empty;
            SelectedRequestType = string.Empty;
            //ValidateDropDownsToGetCreditLimits();
            StateHasChanged();
        }
    }

    private void GenerateGridColumns()
    {
        DataGridColumns = new List<DataGridColumn>
        {
            new DataGridColumn
            {
                Header = "Division",
                GetValue = item =>
                    ((IStoreCreditLimit)item).Division
            },
            new DataGridColumn
            {
                Header = "Credit Limit", GetValue = item => CommonFunctions.RoundForSystemWithoutZero(((IStoreCreditLimit)item).CreditLimit, _appSetting.RoundOffDecimal)
            },
            new DataGridColumn
            {
                Header = "Temp Credit Limit", GetValue = item => CommonFunctions.RoundForSystemWithoutZero(((IStoreCreditLimit)item).TemporaryCreditLimit, _appSetting.RoundOffDecimal)
            },
            new DataGridColumn
            {
                Header = "Existing Out Standing", GetValue = item => CommonFunctions.RoundForSystemWithoutZero(((IStoreCreditLimit)item).CurrentOutstanding, _appSetting.RoundOffDecimal)
            },
            new DataGridColumn
            {
                Header = "Max Aging Days", GetValue = item => ((IStoreCreditLimit)item).MaxAgingDays
            },
            new DataGridColumn
            {
                Header = "Credit Days", GetValue = item => ((IStoreCreditLimit)item).CreditDays
            },
            new DataGridColumn
            {
                Header = "Temp Credit Days", GetValue = item => ((IStoreCreditLimit)item).TemporaryCreditDays
            },
        };
    }
    private async void ValidateDropDownsToGetCreditLimits()
    {
        if (IsView) return;
        if (!string.IsNullOrEmpty(CPUIDForCredit))
        {
            ShowLoader();
            await _viewModel.GetCreditLimitsByChannelPartnerAndDivision(CPUIDForCredit, DivUIDForCredit);
            HideLoader();
        }
        else
        {
            _viewModel.CreditLimits.Clear();
        }
        StateHasChanged();
    }

    public void OnRequestTypeSelect(DropDownEvent dropDownEvent)
    {

        if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            var selectedItem = dropDownEvent.SelectionItems.FirstOrDefault();
            if (selectedItem != null)
            {
                SelectedRequestType = selectedItem.Label ?? string.Empty;
                if (CheckIfRequestExists())
                {
                    ShowErrorSnackBar("Error", "Request Type: " + SelectedRequestType + " For " + _viewModel.TemporaryCreditEnhancementDetails.OrderNumber + "Is Already Exists.Please select the other Request if Required.");
                    SelectedRequestType = "";
                    foreach (var item in _viewModel.TemporaryCreditEnhancementRequestselectionItems)
                    {
                        item.IsSelected = false;
                    }
                }
                else
                {
                    _viewModel.TemporaryCreditEnhancementDetails.RequestType = SelectedRequestType;
                }
            }
        }
        else
        {
            SelectedRequestType = string.Empty;
        }

        // else
        // {
        //     if (string.IsNullOrEmpty(CPUIDForCredit))
        //     {
        //         ShowErrorSnackBar("Error", "Please select a Channel Partner before selecting a Request Type.");
        //     }
        //     if (string.IsNullOrEmpty(DivUIDForCredit))
        //     {
        //         ShowErrorSnackBar("Error", "Please select a Division before selecting a Request Type.");
        //     }
        //     SelectedRequestType = string.Empty;
        //     _viewModel.TemporaryCreditEnhancementRequestselectionItems
        //         .ForEach(item => item.IsSelected = false);
        //     return;
        // }
    }

    public bool CheckIfRequestExists()
    {
        if (_viewModel.TemporaryCreditEnhancementList != null &&
            _viewModel.TemporaryCreditEnhancementList.Any())
        {
            var exists = _viewModel.TemporaryCreditEnhancementList
                .Any(credit => credit.StoreUID == _viewModel.TemporaryCreditEnhancementDetails.StoreUID &&
                    credit.RequestType == SelectedRequestType && credit.Status == "Pending");

            return exists;
        }
        return false;
    }

    private void AfterDeleteImage()
    {

    }
    public async Task SaveCreditEnhancementDetails()
    {
        ShowLoader();
        if (ValidateData())
        {
            // SaveCreditLimitDoc();
            if (await _viewModel.SaveTemporaryCreditRequest(_viewModel.TemporaryCreditEnhancementDetails))
            {
                ShowSuccessSnackBar("Success", "Details Saved Successfully");
                _navigationManager.NavigateTo("TemporaryCreditEnhancement");
            }
            else
            {
                ShowErrorSnackBar("Error", "Error Saving Data");
            }
        }
        else
        {
            ShowErrorSnackBar(" Error ", ValidationError);
        }
        HideLoader();
    }
    public WinitCalendar? ref1 { get; set; }
    public WinitCalendar? ref2 { get; set; }
    public async Task OnDateChange(CalenderWrappedData calenderWrappedData)
    {
        DateTime selectedDate = DateTime.Parse(calenderWrappedData.SelectedValue);

        if (calenderWrappedData.Id == "EffectiveFrom")
        {
            if (_viewModel.TemporaryCreditEnhancementDetails.EffectiveUpto != null)
            {
                if (selectedDate <= _viewModel.TemporaryCreditEnhancementDetails.EffectiveUpto)
                {
                    _viewModel.TemporaryCreditEnhancementDetails.EffectiveFrom = selectedDate;
                }
                else
                {
                    ShowErrorSnackBar("Error", "Effective-From Date should not be greater than Effective-To Date.");
                    _viewModel.TemporaryCreditEnhancementDetails.EffectiveFrom = null;

                }
            }
            else
            {
                _viewModel.TemporaryCreditEnhancementDetails.EffectiveFrom = selectedDate;
            }
        }
        else if (calenderWrappedData.Id == "EffectiveTo")
        {
            if (_viewModel.TemporaryCreditEnhancementDetails.EffectiveFrom != null)
            {
                if (selectedDate >= _viewModel.TemporaryCreditEnhancementDetails.EffectiveFrom)
                {
                    _viewModel.TemporaryCreditEnhancementDetails.EffectiveUpto = selectedDate;
                }
                else
                {
                    ShowErrorSnackBar("Error", "Effective-To Date should not be lower than Effective-From Date.");
                    _viewModel.TemporaryCreditEnhancementDetails.EffectiveUpto = null;

                }
            }
            else
            {
                _viewModel.TemporaryCreditEnhancementDetails.EffectiveUpto = selectedDate;
            }
        }
    }

   
    private bool ValidateData()
    {
        ValidationError = string.Empty;
        if (!string.IsNullOrEmpty(SelectedRequestType))
        {
            if (SelectedRequestType == "Credit Limit")
            {
                //_viewModel.TemporaryCreditEnhancementDetails.TempCreditDays = null;
                // if ((_viewModel.TemporaryCreditEnhancementDetails.EffectiveFrom) == null)
                // {
                //     ValidationError += "Effective From Date Should not be Null.";
                // }
                // if ((_viewModel.TemporaryCreditEnhancementDetails.EffectiveUpto) == null)
                // {
                //     ValidationError += "Effective To Date Should not be Null.";
                // }
                if (string.IsNullOrWhiteSpace(_viewModel.TemporaryCreditEnhancementDetails.RequestAmountDays.ToString()))
                {
                    ValidationError += "Incremental Credit Limit Should not be Null or Empty.";
                }
            }
            else if (SelectedRequestType == "Aging Days")
            {
                _viewModel.TemporaryCreditEnhancementDetails.EffectiveFrom = null;
                _viewModel.TemporaryCreditEnhancementDetails.EffectiveUpto = null;
                if (string.IsNullOrWhiteSpace(_viewModel.TemporaryCreditEnhancementDetails.RequestAmountDays.ToString()))
                {
                    ValidationError += "Incremental Aging Days Should not be Null or Empty.";
                }
            }
        }
        else
        {
            ValidationError += "Request Type Cannot be Null .";
        }
        if (string.IsNullOrWhiteSpace(_viewModel.TemporaryCreditEnhancementDetails.Remarks))
        {
            ValidationError += "Remarks Should not be Null or Empty.";
        }
        //if (CreditEnhancementDocUpload == null || CreditEnhancementSysList == null || CreditEnhancementSysList.Any())
        //{
        //    ValidationError += "Attatchments not Uploaded.";
        //}
        if (ValidationError.Length > 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    private void BackBtnClicked()
    {
        ShowLoader();
        _navigationManager.NavigateTo($"TemporaryCreditEnhancement");
        HideLoader();
    }
    private void OnAgingDaysInput(ChangeEventArgs e)
    {

        if (decimal.TryParse(e.Value?.ToString(), out decimal result) && result <= _appSetting.TempAgingDaysMaxDays)
        {
            _viewModel.TemporaryCreditEnhancementDetails.RequestAmountDays = result;
        }
        else
        {
            e.Value = _viewModel.TemporaryCreditEnhancementDetails.RequestAmountDays;
        }
        StateHasChanged();
    }
    protected async Task SaveFileSys(List<IFileSys> fileSysList)
    {
        if (fileSysList == null || !fileSysList.Any())
        {
            await _alertService.ShowErrorAlert("Error", "Please Upload Files");
            //_tost.Add("SKU Image", "SKU Image Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
            return;
        }

        Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await fileUploader.MoveFiles();
        if (apiResponse.IsSuccess)
        {
            _viewModel.CreditEnhancementFileSysList = _viewModel.CreditEnhancementFileSysList.Where(x => x.Id < 0).ToList();
            //await SaveOrUpdateCustomerInformation.InvokeAsync(_onBoardCustomerDTO);

            //if (IsSuccessCustomerInformation)
            //{
            //    ButtonName = "Update";
            //}
        }
        else
        {

        }
    }
}
