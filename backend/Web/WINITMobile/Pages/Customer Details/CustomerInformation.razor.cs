using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Constants;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.UIModels.Common;
using Winit.UIModels.Common.GST;
using WINITMobile.Pages.Base;


namespace WINITMobile.Pages.Customer_Details
{
    public partial class CustomerInformation : BaseComponentBase
    {

        [Parameter]
        public List<ISelectionItem> FirmTypeselectionItems { get; set; } = new List<ISelectionItem>
        {
            new SelectionItem { UID = "1", Label = FirmTypeConstants.Propertier},
            new SelectionItem { UID = "2", Label = FirmTypeConstants.Partnership},
            new SelectionItem { UID = "3", Label = FirmTypeConstants.Company },

            };
        public string ValidationMessage;
        private bool IsSaveAttempted { get; set; } = false;
        [Parameter] public List<ISelectionItem> CustomerClassificationselectionItems { get; set; }
        [Parameter] public List<ISelectionItem> ClassificationTypeselectionItems { get; set; }
        [Parameter] public IOnBoardCustomerDTO? OriginalOnBoardCustomerDTO { get; set; }
        [Parameter] public IOnBoardCustomerDTO? _onBoardCustomerDTO { get; set; }
        [Parameter] public EventCallback<IOnBoardCustomerDTO> SaveOrUpdateCustomerInformation { get; set; }
        [Parameter] public EventCallback<string> InsertDataInChangeRequest { get; set; }
        [Parameter] public EventCallback<string> BroadClassificationSelection { get; set; }
        [Parameter] public EventCallback<string> OnChangeBroadClassification { get; set; }
        [Parameter] public EventCallback<string> FirmTypeSelection { get; set; }
        [Parameter] public EventCallback<bool> IsAsmMappedByCustomer { get; set; }
        [Parameter] public EventCallback<IOnBoardCustomerDTO> SaveOrUpdateFileSys { get; set; }
        [Parameter] public string CustomerCode { get; set; }

        private System.Timers.Timer _timer;
        public bool IsFirmTypeEmpty { get; set; }
        private Winit.UIComponents.Common.FileUploader.FileUploader? fileUploader { get; set; }
        private string? FilePath { get; set; }
        private List<IFileSys>? fileSysList { get; set; }
        [Parameter] public EventCallback<bool> MSME { get; set; }
        [Parameter] public EventCallback<bool> Vendor { get; set; }
        public bool Isinitialized { get; set; } = true;
        [Parameter] public GSTINDetailsModel onBoardCustomerDTODataSource { get; set; }
        [Parameter]
        public GSTINDetailsModel GSTINDetailsDataSource { get; set; }
        [Parameter] public bool IsEditOnBoardDetails { get; set; }

        [Parameter] public bool CustomerEditApprovalRequired { get; set; }
        public string ButtonName { get; set; } = "Save";
        [Parameter] public string TabName { get; set; }
        //[Parameter]
        //public bool IsSuccessCustomerInformation { get; set; }
        public bool IsSuccessCustInfn { get; set; }


        [Parameter] public List<ISelectionItem> DivisionselectionItems { get; set; }
        public string SelectedDivisions { get; set; } = "N/A";
        public bool IsNew { get; set; } = true;
        [Parameter] public bool IsSelfRegistration { get; set; }
        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            IsSuccessCustInfn = false;
            ButtonName = IsEditOnBoardDetails ? "Update" : "Save";
            FilePath = FileSysTemplateControles.GetOnBoardImageCheckFolderPath(_onBoardCustomerDTO.Store.UID);
            if (IsEditOnBoardDetails)
            {
                await MapGSTonEdit();
                SelectedDivisions = string.Join(", ", DivisionselectionItems.Where(p => p.IsSelected).Select(p => p.Label).ToList());
                await IsAsmMappedByCustomer.InvokeAsync(_onBoardCustomerDTO.Store.IsAsmMappedByCustomer);
            }
            else
            {
                _onBoardCustomerDTO.StoreAdditionalInfo.IsMSME = true;
                _onBoardCustomerDTO.StoreAdditionalInfo.IsVendor = true;
                await IsAsmMappedByCustomer.InvokeAsync(_onBoardCustomerDTO.Store.IsAsmMappedByCustomer = true);
            }
            _loadingService.HideLoading();
            StateHasChanged();
        }

        public string fullAddress { get; set; }
        public string filteredGstAddress1 { get; set; }
        public string filteredGstAddress2 { get; set; }
        public string GstAddressMapping()
        {
            if (IsEditOnBoardDetails)
            {
                return _onBoardCustomerDTO.StoreAdditionalInfo.GSTAddress;
            }
            else
            {
                var gstAddress1 = new List<string>
                {
                    GSTINDetailsDataSource?.PR_ADR_DoorNo,
                    GSTINDetailsDataSource?.PR_ADR_FloorNo,
                    GSTINDetailsDataSource?.PR_ADDR_BuildingName
                };

                var gstAddress2 = new List<string>
                {
                    GSTINDetailsDataSource?.PR_ADR_Street,
                    GSTINDetailsDataSource?.PR_ADR_Location
                };

                var addressParts = new List<string>();

                // Combine gstAddress1 parts, skipping null or empty values
                addressParts.AddRange(gstAddress1.Where(part => !string.IsNullOrWhiteSpace(part)));

                // Combine gstAddress2 parts, skipping null or empty values
                addressParts.AddRange(gstAddress2.Where(part => !string.IsNullOrWhiteSpace(part)));

                // Add additional address fields, skipping null or empty values
                addressParts.AddRange(new List<string>
                {
                    GSTINDetailsDataSource?.PR_ADR_Landmark,
                    GSTINDetailsDataSource?.PR_ADR_District,
                    GSTINDetailsDataSource?.PR_ADR_Locality,
                    GSTINDetailsDataSource?.PR_ADR_State,
                    GSTINDetailsDataSource?.PR_ADR_Pincode
                }.Where(part => !string.IsNullOrWhiteSpace(part)));


                //var filteredAddressParts = addressParts.Where(part => !string.IsNullOrEmpty(part));
                fullAddress = string.Join(", ", addressParts);
                filteredGstAddress1 = string.Join(", ", gstAddress1);
                filteredGstAddress2 = string.Join(", ", gstAddress2);
                _onBoardCustomerDTO.StoreAdditionalInfo.GSTAddress = fullAddress;
                _onBoardCustomerDTO.StoreAdditionalInfo.GSTAddress1 = filteredGstAddress1;
                _onBoardCustomerDTO.StoreAdditionalInfo.GSTAddress2 = filteredGstAddress2;
                _onBoardCustomerDTO.StoreAdditionalInfo.GSTDistrict = GSTINDetailsDataSource?.PR_ADR_District;
                return fullAddress;
            }
        }
        //public void MapGSTonAdd()
        //{
        //    try
        //    {
        //        GSTINDetailsDataSource.GSTIN =onBoardCustomerDTODataSource.Store.GSTNo;
        //        GSTINDetailsDataSource.LegalName = onBoardCustomerDTODataSource.Store.LegalName;
        //        GSTINDetailsDataSource.TradeName = onBoardCustomerDTODataSource.Store.TradeName;
        //        GSTINDetailsDataSource.Status = onBoardCustomerDTODataSource.Store.Status;
        //        GSTINDetailsDataSource.PR_NatureOfBusiness = onBoardCustomerDTODataSource.StoreAdditionalInfo.NatureOfBusiness;
        //        //GSTINDetailsDataSource.PR_ADR_State = onBoardCustomerDTODataSource.StoreAdditionalInfo.sta;
        //        GSTINDetailsDataSource.PR_ADR_Pincode = onBoardCustomerDTODataSource.StoreAdditionalInfo.PinCode;
        //        GSTINDetailsDataSource.RegistrationDate = onBoardCustomerDTODataSource.StoreAdditionalInfo.DateOfRegistration;
        //        GSTINDetailsDataSource.PR_ADR_DoorNo = onBoardCustomerDTODataSource.StoreAdditionalInfo.GSTAddress;
        //        GSTINDetailsDataSource.PR_ADR_FloorNo= onBoardCustomerDTODataSource.StoreAdditionalInfo.GSTAddress;
        //        GSTINDetailsDataSource.PR_ADDR_BuildingName= onBoardCustomerDTODataSource.StoreAdditionalInfo.GSTAddress;
        //        GSTINDetailsDataSource.PR_ADR_Street= onBoardCustomerDTODataSource.StoreAdditionalInfo.GSTAddress;
        //        GSTINDetailsDataSource.PR_ADR_Location= onBoardCustomerDTODataSource.StoreAdditionalInfo.GSTAddress;
        //        StateHasChanged();
        //    }
        //    catch(Exception ex)
        //    {

        //    }
        //}
        public async Task MapGSTonEdit()
        {
            try
            {
                GSTINDetailsDataSource.GSTIN = _onBoardCustomerDTO.Store.GSTNo;
                GSTINDetailsDataSource.LegalName = _onBoardCustomerDTO.Store.LegalName;
                GSTINDetailsDataSource.TradeName = _onBoardCustomerDTO.Store.TradeName;
                GSTINDetailsDataSource.Status = _onBoardCustomerDTO.StoreAdditionalInfo.GSTINStatus;
                GSTINDetailsDataSource.PR_NatureOfBusiness = _onBoardCustomerDTO.StoreAdditionalInfo.NatureOfBusiness;
                GSTINDetailsDataSource.Duty = _onBoardCustomerDTO.StoreAdditionalInfo.TaxPaymentType;
                GSTINDetailsDataSource.PR_ADR_State = _onBoardCustomerDTO.StoreAdditionalInfo.GSTState;
                GSTINDetailsDataSource.PR_ADR_Pincode = _onBoardCustomerDTO.StoreAdditionalInfo.PinCode;
                GSTINDetailsDataSource.RegistrationDate = _onBoardCustomerDTO.StoreAdditionalInfo.DateOfRegistration;
                // GSTINDetailsDataSource.Duty = _onBoardCustomerDTO.StoreAdditionalInfo.du;
                GstAddressMapping();


            }
            catch (Exception ex)
            {

            }
        }

        private async Task IsAsmMappedChanged(ChangeEventArgs IsMappedByCustomer)
        {
            await IsAsmMappedByCustomer.InvokeAsync(_onBoardCustomerDTO.Store.IsAsmMappedByCustomer = Convert.ToBoolean(IsMappedByCustomer.Value));
            StateHasChanged();
        }
        private void OnMCMEChange(bool IsMsme)
        {
            // Update the StoreAdditionalInfo property directly
            _onBoardCustomerDTO.StoreAdditionalInfo.IsMSME = IsMsme;
            MSME.InvokeAsync(_onBoardCustomerDTO.StoreAdditionalInfo.IsMSME);
            StateHasChanged();
        }
        private void OnVendorChange(bool Isvendor)
        {
            // Update the StoreAdditionalInfo property directly
            _onBoardCustomerDTO.StoreAdditionalInfo.IsVendor = Isvendor;
            Vendor.InvokeAsync(_onBoardCustomerDTO.StoreAdditionalInfo.IsVendor);
            StateHasChanged();
        }
        private bool IsBroadCustomerClassificationSelectionValid()
        {
            return !string.IsNullOrEmpty(_onBoardCustomerDTO.Store.BroadClassification);
        }
        private bool IsStoreCredit()
        {
            if (_onBoardCustomerDTO.StoreCredit.Any(p => p.IsActive))
            {
                return false;
            }
            return true;
        }
        public async Task OnBroadCustomerClassificationSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                _onBoardCustomerDTO.Store.BroadClassification = null;
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                await BroadClassificationSelection.InvokeAsync(selecetedValue?.Label);
                await OnChangeBroadClassification.InvokeAsync(selecetedValue.UID);
                _onBoardCustomerDTO.Store.BroadClassification = selecetedValue?.Label;
                StateHasChanged();
            }
            else
            {
                _onBoardCustomerDTO.Store.BroadClassification = string.Empty;
            }
        }

        public async Task OnBroadCustomerClassificationTypeSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            _onBoardCustomerDTO.Store.ClassficationType = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                // sku.Code = selecetedValue?.Code;
                _onBoardCustomerDTO.Store.ClassficationType = selecetedValue?.Label;
                StateHasChanged();
            }
        }

        public async Task OnDivisionSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                SelectedDivisions = string.Join(", ", dropDownEvent.SelectionItems.Select(item => item.Label));
                if (IsNew && !IsEditOnBoardDetails)
                {
                    _onBoardCustomerDTO.StoreCredit = new List<IStoreCredit>();

                    foreach (var item in dropDownEvent.SelectionItems)
                    {
                        StoreCredit storeCredit = new StoreCredit();
                        storeCredit.DivisionOrgUID = item.UID;
                        storeCredit.IsActive = true;
                        _onBoardCustomerDTO.StoreCredit.Add(storeCredit);
                    }
                }
                else
                {
                    _onBoardCustomerDTO.StoreCredit.ForEach(m =>
                    {
                        m.IsActive = dropDownEvent.SelectionItems.Any(p => p.UID == m.DivisionOrgUID);
                    });

                    foreach (var item in dropDownEvent.SelectionItems.Where(p => !_onBoardCustomerDTO.StoreCredit.Any(q => p.UID == q.DivisionOrgUID)))
                    {
                        StoreCredit storeCredit = new StoreCredit();
                        storeCredit.DivisionOrgUID = item.UID;
                        _onBoardCustomerDTO.StoreCredit.Add(storeCredit);
                    }
                }

            }
            else
            {
                _onBoardCustomerDTO.StoreCredit.ForEach(m => m.IsActive = false);
            }
        }
        public async Task OnFirmTypeSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                _onBoardCustomerDTO.StoreAdditionalInfo.FirmType = null;
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                // sku.Code = selecetedValue?.Code;
                _onBoardCustomerDTO.StoreAdditionalInfo.FirmType = selecetedValue?.Label;
                await FirmTypeSelection.InvokeAsync(_onBoardCustomerDTO.StoreAdditionalInfo.FirmType);

            }
            else
            {
                _onBoardCustomerDTO.StoreAdditionalInfo.FirmType = string.Empty;
                IsFirmTypeEmpty = true;
            }
        }
        public bool IsValidUrl(string url)
        {
            string pattern = @"^(https?://.+)$";
            return Regex.IsMatch(url, pattern, RegexOptions.IgnoreCase);
        }

        public bool FirmTypeValidation()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_onBoardCustomerDTO.StoreAdditionalInfo.FirmType))
                {
                    return true;
                }
                else
                {
                    if (_onBoardCustomerDTO.StoreAdditionalInfo.FirmType == FirmTypeConstants.Company)
                    {
                        return string.IsNullOrWhiteSpace(_onBoardCustomerDTO?.StoreAdditionalInfo?.FirmRegNo) ||
                    string.IsNullOrWhiteSpace(_onBoardCustomerDTO.StoreAdditionalInfo.CompanyRegNo);
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                return true;
            }
        }
        public string ShowValidationStar()
        {
            try
            {
                if (_onBoardCustomerDTO?.StoreAdditionalInfo?.FirmType == FirmTypeConstants.Company)
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
        protected async Task SaveOrUpdate()
        {

            IsSaveAttempted = true;
            try
            {
                ValidationMessage = null;
                if (
                    !IsBroadCustomerClassificationSelectionValid() ||
                    FirmTypeValidation() ||
                    (IsStoreCredit() && !IsSelfRegistration))
                //CheckValidWebsite(_onBoardCustomerDTO.StoreAdditionalInfo.WebSite))

                {
                    ValidationMessage = "The Following fields has invalid field(s)" + ": ";

                    if (!IsBroadCustomerClassificationSelectionValid())
                    {
                        ValidationMessage += "Broad Customer Classification, ";
                    }
                    if (string.IsNullOrWhiteSpace(_onBoardCustomerDTO?.StoreAdditionalInfo?.FirmType))
                    {
                        ValidationMessage += "Type of Firm, ";
                    }
                    if (_onBoardCustomerDTO?.StoreAdditionalInfo?.FirmType == FirmTypeConstants.Company)
                    {
                        if (string.IsNullOrWhiteSpace(_onBoardCustomerDTO?.StoreAdditionalInfo?.FirmRegNo))
                        {
                            ValidationMessage += "Firm Registration Number, ";
                        }
                        if (string.IsNullOrWhiteSpace(_onBoardCustomerDTO?.StoreAdditionalInfo?.CompanyRegNo))
                        {
                            ValidationMessage += "Company Registration Number, ";
                        }
                    }


                    if (IsStoreCredit() && !IsSelfRegistration)
                    {
                        ValidationMessage += "Division Details, ";
                    }
                    //if (CheckValidWebsite(_onBoardCustomerDTO?.StoreAdditionalInfo?.WebSite))
                    //{
                    //    ValidationMessage += "WebSite";
                    //}
                    ValidationMessage = ValidationMessage.TrimEnd(' ', ',');
                }
                else
                {
                    IOnBoardCustomerDTO empDTO = new OnBoardCustomerDTO();
                    if (_onBoardCustomerDTO.FileSys.Count > 0 && TabName != StoreConstants.Confirmed)
                    {
                        await SaveFileSys();
                    }
                    else if (_onBoardCustomerDTO.FileSys.Count == 0 && TabName != StoreConstants.Confirmed)
                    {
                        await SaveOrUpdateCustomerInformation.InvokeAsync(_onBoardCustomerDTO);
                    }
                    else
                    {
                        if (TabName == StoreConstants.Confirmed && !CustomerEditApprovalRequired)
                        {
                            await RequestChange(OriginalOnBoardCustomerDTO, _onBoardCustomerDTO);
                            await SaveOrUpdateCustomerInformation.InvokeAsync(_onBoardCustomerDTO);
                        }
                        else if (TabName == StoreConstants.Confirmed)
                        {
                            await RequestChange(OriginalOnBoardCustomerDTO, _onBoardCustomerDTO);
                        }
                        //if(IsSuccessCustomerInformation)
                        //{
                        //    ButtonName = "Update";
                        //}
                    }
                    IsSuccessCustInfn = true;
                    IsNew = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        #region RequestChange
        public async Task RequestChange(IOnBoardCustomerDTO originalObj, IOnBoardCustomerDTO modifiedObj)
        {
            // Directly creating List<IChangeRecordDTO> using inline creation of ChangeRecordDTOs
            List<IChangeRecordDTO> ChangeRecordDTOs = new List<IChangeRecordDTO>
            {
                new ChangeRecordDTO
                {
                     Action= OnboardingScreenConstant.Update,
                    ScreenModelName = OnboardingScreenConstant.CustomInfoStore,
                    UID = originalObj?.Store?.UID!,
                    ChangeRecords = CommonFunctions.GetChangedData(CommonFunctions.CompareObjects(originalObj?.Store, modifiedObj?.Store)!)
                },
                new ChangeRecordDTO
                {
                     Action= OnboardingScreenConstant.Update,
                    ScreenModelName = OnboardingScreenConstant.CustomInfoStoreAdditionInfo,
                    UID = originalObj?.StoreAdditionalInfo?.UID!,
                    ChangeRecords = CommonFunctions.GetChangedData(CommonFunctions.CompareObjects(originalObj?.StoreAdditionalInfo, modifiedObj?.StoreAdditionalInfo)!)
                },
                new ChangeRecordDTO
                {
                     Action= OnboardingScreenConstant.Update,
                    ScreenModelName = OnboardingScreenConstant.CustomInfoStoreAdditionInfoCmi,
                    UID = originalObj?.StoreAdditionalInfoCMI?.UID!,
                    ChangeRecords = CommonFunctions.GetChangedData(CommonFunctions.CompareObjects(originalObj?.StoreAdditionalInfoCMI, modifiedObj?.StoreAdditionalInfoCMI)!)
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

        public object GetModifiedObject(IOnBoardCustomerDTO onBoardCustomerDTO)
        {
            var modifiedObject = new
            {
                onBoardCustomerDTO.Store,
                onBoardCustomerDTO.StoreAdditionalInfo,
                onBoardCustomerDTO.StoreAdditionalInfoCMI
            };

            return modifiedObject;
        }
        #endregion
        public bool CheckValidWebsite(string url)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(url))
                {
                    return !IsValidUrl(url);
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private void GetsavedImagePath(List<IFileSys> ImagePath)
        {
            _onBoardCustomerDTO.FileSys = ImagePath;
        }
        private void AfterDeleteImage()
        {

        }
        protected async Task SaveFileSys()
        {
            if (_onBoardCustomerDTO.FileSys == null || !_onBoardCustomerDTO.FileSys.Any())
            {
                await _alertService.ShowErrorAlert("Error", "Please Upload Files");
                //_tost.Add("SKU Image", "SKU Image Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                return;
            }

            Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await fileUploader.MoveFiles();
            if (apiResponse.IsSuccess)
            {
                _onBoardCustomerDTO.FileSys = _onBoardCustomerDTO.FileSys.Where(x => x.Id < 0).ToList();
                await SaveOrUpdateCustomerInformation.InvokeAsync(_onBoardCustomerDTO);

                //if (IsSuccessCustomerInformation)
                //{
                //    ButtonName = "Update";
                //}
            }
            else
            {

            }
        }


        protected override void OnAfterRender(bool firstRender)
        {
            if (!string.IsNullOrWhiteSpace(ValidationMessage))
            {
                StartTimer();
            }
        }

        private void StartTimer()
        {
            _timer = new System.Timers.Timer(3000); // Set the timer for 3 seconds
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = false; // Ensure the timer runs only once
            _timer.Start();
        }

        private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timer.Stop();
            _timer.Dispose();
            _timer = null;

            // Clear the validation message
            ValidationMessage = string.Empty;

            // Trigger a re-render
            InvokeAsync(StateHasChanged);
        }

        public void OnDateChange(CalenderWrappedData calenderWrappedData)
        {
            if (calenderWrappedData.Id == "DateOfFoundation")
            {

                _onBoardCustomerDTO.StoreAdditionalInfoCMI.DateOfFoundation = DateTime.Parse(calenderWrappedData.SelectedValue);
            }
            //StateHasChanged();
            else if (calenderWrappedData.Id == "DateOfBirth")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.DateOfBirth = DateTime.Parse(calenderWrappedData.SelectedValue);
            }
        }
        //public async Task CreateUpdateSKUImage(List<IFileSys> fileSys)
        //{
        //    if (fileSys.Count > 0)
        //    {
        //        await SaveOrUpdateFileSys.InvokeAsync();
        //    }
        //    else
        //    {
        //        return ;
        //    }
        //    HideLoader();
        //} 
        #region PAN number Fetching Logic
        private string GetPanFromGst(string gstin)
        {
            // Ensure the GSTIN is not null or empty, and it has at least 10 characters
            if (!string.IsNullOrEmpty(gstin) && gstin.Length >= 10)
            {
                return gstin.Substring(OnboardingScreenConstant.PanStatrtIndex, OnboardingScreenConstant.PanEndIndex); //do not change this
            }
            return "Invalid GSTIN";  // If GSTIN is invalid or doesn't contain PAN
        }
        #endregion
    }
}
