using Microsoft.AspNetCore.Components;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Constants;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using WINITMobile.Pages.Base;

namespace WINITMobile.Pages.Customer_Details
{
    public partial class EarlierWorkedWithCMI : BaseComponentBase
    {
        public string ValidationMessage;

        private bool IsSaveAttempted { get; set; } = false;
        private bool isYesChecked = true;
        private bool isNoChecked = false;
        [Parameter] public IStoreAdditionalInfoCMI StoreAdditionalInfoCMI { get; set; } = new StoreAdditionalInfoCMI();
        [Parameter] public IStoreAdditionalInfoCMI OriginalStoreAdditionalInfoCMI { get; set; }
        [Parameter] public EventCallback<IStoreAdditionalInfoCMI> SaveandUpdate { get; set; }
        [Parameter] public EventCallback<bool?> EarlierWorkedWithCmi { get; set; }
        public string ButtonName { get; set; } = "Save";
        [Parameter] public bool IsEditOnBoardEditDetails { get; set; }
        public bool IsSuccess { get; set; } = false;
        [Parameter] public EventCallback<string> InsertDataInChangeRequest { get; set; }
        [Parameter] public string StoreAdditionalInfoCMIUid { get; set; }
        [Parameter] public bool CustomerEditApprovalRequired { get; set; }
        [Parameter] public string TabName { get; set; }
        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            if (!IsEditOnBoardEditDetails)
            {
                StoreAdditionalInfoCMI.EwwHasWorkedWithCMI = true;
            }
            else
            {

            }

            ButtonName = IsEditOnBoardEditDetails ? "Update" : "Save";
            if (TabName == StoreConstants.Confirmed)
            {
                var concreteAddress = StoreAdditionalInfoCMI as StoreAdditionalInfoCMI;
                OriginalStoreAdditionalInfoCMI = concreteAddress.DeepCopy()!;
            }
            await Task.CompletedTask;
            _loadingService.HideLoading();
        }
        protected async Task OnClean()
        {

            StoreAdditionalInfoCMI = new StoreAdditionalInfoCMI
            {
                EwwHasWorkedWithCMI = null,
                EwwYearOfOperationAndVolume = string.Empty,
                EwwDealerInfo = string.Empty,
                EwwNameOfFirms = string.Empty,
                EwwTotalInvestment = null,
            };
            StateHasChanged();
        }
        public async Task SaveOrUpdate()
        {
            await EarlierWorkedWithCmi.InvokeAsync(StoreAdditionalInfoCMI.EwwHasWorkedWithCMI);
            //IsSaveAttempted = true;
            if (StoreAdditionalInfoCMI.EwwHasWorkedWithCMI == null)
            {
                await _alertService.ShowErrorAlert(OnboardingScreenConstant.EarlierWorkWithCMI, "Selection cannot be Empty");
                return;
            }
            if (StoreAdditionalInfoCMI.EwwHasWorkedWithCMI == true)
            {
                ValidateAllFields();
            }
            if (StoreAdditionalInfoCMI.EwwHasWorkedWithCMI == false)
            {
                ValidateSingleField();
            }
            if (string.IsNullOrWhiteSpace(ValidationMessage))
            {
                try
                {

                    StoreAdditionalInfoCMI.SectionName = OnboardingScreenConstant.EarlierWorkWithCMI;
                    if (TabName == StoreConstants.Confirmed && !CustomerEditApprovalRequired)
                    {
                        await RequestChange();
                        await SaveandUpdate.InvokeAsync(StoreAdditionalInfoCMI);
                    }
                    else if (TabName == StoreConstants.Confirmed && CustomerEditApprovalRequired)
                    {
                        await RequestChange();
                    }
                    else
                    {
                        await SaveandUpdate.InvokeAsync(StoreAdditionalInfoCMI);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            IsSuccess = true;
            StateHasChanged();
        }
        private void ValidateAllFields()
        {
            ValidationMessage = null;

            if (string.IsNullOrWhiteSpace(StoreAdditionalInfoCMI.EwwYearOfOperationAndVolume) ||
                string.IsNullOrWhiteSpace(StoreAdditionalInfoCMI.EwwDealerInfo) ||
                string.IsNullOrWhiteSpace(StoreAdditionalInfoCMI.EwwNameOfFirms))
            {
                ValidationMessage = "The following fields have invalid field(s)" + ": ";

                if (string.IsNullOrWhiteSpace(StoreAdditionalInfoCMI.EwwYearOfOperationAndVolume))
                {
                    ValidationMessage += "Year Of Operation And Volume, ";
                }

                if (string.IsNullOrWhiteSpace(StoreAdditionalInfoCMI.EwwDealerInfo))
                {
                    ValidationMessage += "Dealer Info, ";
                }
                if (string.IsNullOrWhiteSpace(StoreAdditionalInfoCMI.EwwNameOfFirms))
                {
                    ValidationMessage += "Name Of Firms, ";
                }

                ValidationMessage = ValidationMessage.TrimEnd(' ', ',');
            }
        }
        private void ValidateSingleField()
        {
            ValidationMessage = null;

            if (string.IsNullOrWhiteSpace(StoreAdditionalInfoCMI.EwwNameOfFirms))
            {
                ValidationMessage = "The following fields have invalid field(s)" + ": ";

                if (string.IsNullOrWhiteSpace(StoreAdditionalInfoCMI.EwwNameOfFirms))
                {
                    ValidationMessage += "Name Of Firms, ";
                }

                ValidationMessage = ValidationMessage.TrimEnd(' ', ',');
            }
        }
        private void OnCMIChange(ChangeEventArgs e)
        {
            // Update the StoreAdditionalInfo property directly
            StoreAdditionalInfoCMI.EwwHasWorkedWithCMI = e.Value.ToString() == "true";
        }
        public string ShowValidationStar()
        {
            try
            {
                if (StoreAdditionalInfoCMI.EwwHasWorkedWithCMI == true)
                {
                    return "*";
                }
                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }
        #region Change RequestLogic

        public async Task RequestChange()
        {
            List<IChangeRecordDTO> ChangeRecordDTOs = new List<IChangeRecordDTO>
            {
                new ChangeRecordDTO
                {
                     Action= OnboardingScreenConstant.Update,
                    ScreenModelName = OnboardingScreenConstant.EarlierWorkWithCMI,
                    UID = StoreAdditionalInfoCMIUid,
                    ChangeRecords = CommonFunctions.GetChangedData(CommonFunctions.CompareObjects(OriginalStoreAdditionalInfoCMI!, StoreAdditionalInfoCMI)!)
                }
            }
            .Where(c => c.ChangeRecords.Count > 0)
            .ToList();

            if (ChangeRecordDTOs.Count > 0)
            {
                var ChangeRecordDTOInJson = CommonFunctions.ConvertToJson(ChangeRecordDTOs);
                await InsertDataInChangeRequest.InvokeAsync(ChangeRecordDTOInJson);
            }
            ChangeRecordDTOs.Clear();
        }
        public object GetModifiedObject(IStoreAdditionalInfoCMI storeAdditionalinfoCMI)
        {
            var modifiedObject = new
            {
                storeAdditionalinfoCMI.EwwHasWorkedWithCMI,
                storeAdditionalinfoCMI.EwwYearOfOperationAndVolume,
                storeAdditionalinfoCMI.EwwDealerInfo,
                storeAdditionalinfoCMI.EwwNameOfFirms,
                storeAdditionalinfoCMI.EwwTotalInvestment
            };

            return modifiedObject;
        }
        #endregion
    }
}
