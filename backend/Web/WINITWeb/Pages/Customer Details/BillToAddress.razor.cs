using Microsoft.AspNetCore.Components;
using Winit.Modules.Address.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.UIModels.Common.GST;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Winit.Modules.Contact.Model.Interfaces;
using Winit.Shared.Models.Constants;
using Winit.Modules.Contact.Model.Classes;
using Winit.Modules.Address.Model.Classes;
using Winit.Modules.Store.Model.Constants;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using DocumentFormat.OpenXml.Wordprocessing;


namespace WinIt.Pages.Customer_Details
{
    public partial class BillToAddress
    {
        public List<ISelectionItem> selectionItems = new();
        public string ValidationMessage;
        [Parameter] public IAddress _locationInformation { get; set; }
        protected List<Winit.Modules.Location.Model.Classes.LocationMaster>? LocationMasters;
        [Inject] Winit.Modules.Setting.BL.Interfaces.IAppSetting _appSettings { get; set; }
        public Winit.Modules.Location.Model.Interfaces.ILocationData SelectedLocation { get; set; }
        private bool ViewLocation { get; set; }
        [Parameter] public List<ISelectionItem> GenderselectionItems { get; set; }
        [Parameter] public List<ISelectionItem> StateselectionItems { get; set; }
        [Parameter] public List<ISelectionItem> CityselectionItems { get; set; }
        [Parameter] public List<ISelectionItem> LocalityselectionItems { get; set; }
        [Parameter] public List<ISelectionItem> PinCodeselectionItems { get; set; }
        [Parameter] public List<ISelectionItem> BranchselectionItems { get; set; }
        [Parameter] public List<ISelectionItem> OUselectionItems { get; set; }
        [Parameter] public List<ISelectionItem> ASMselectionItems { get; set; }
        [Parameter] public List<ISelectionItem> SalesOfficeselectionItems { get; set; }
        private bool IsSaveAttempted { get; set; } = false;
        public List<DataGridColumn> DataGridColumns { get; set; }
        public IAddress SelectedAddress { get; set; }
        public IAddress OriginalSelectedAddress { get; set; }
        [Parameter] public EventCallback<IAddress> SaveOrUpdateBilltoAddress { get; set; }
        [Parameter] public EventCallback<IAddress> OnAddBilltoAddress { get; set; }
        [Parameter] public EventCallback<IAddress> OnEditBilltoAddress { get; set; }
        [Parameter] public EventCallback<string> OnDelete { get; set; }
        [Parameter] public string SelfRegistrationUID { get; set; }
        [Parameter] public EventCallback<List<string>> StateSelected { get; set; }
        [Parameter] public EventCallback<string> InsertDataInChangeRequest { get; set; }
        [Parameter] public EventCallback<List<string>> CitySelected { get; set; }
        [Parameter] public EventCallback<List<string>> LocalitySelected { get; set; }
        [Parameter] public EventCallback<string> BranchSelected { get; set; }
        [Parameter] public EventCallback<string> AsmSelected { get; set; }
        [Parameter] public EventCallback<string> OnEditClickInvokeAddressDropdowns { get; set; }
        [Parameter] public EventCallback<IAddress> OnEditInvokeLocationDropdowns { get; set; }
        [Parameter] public EventCallback<GSTINDetailsModel> OnCopyGSTLocationDropdowns { get; set; }
        [Parameter] public Func<Task<List<IAddress>>> OnShowAllBilltoAddressClick { get; set; }
        [Parameter] public GSTINDetailsModel GSTINDetailsModel { get; set; }
        [Parameter] public IStoreAdditionalInfo StoreAdditionalInfo { get; set; }
        [Parameter] public bool IsBillToAddressSuccess { get; set; }
        [Parameter] public string TabName { get; set; }
        [Parameter] public List<IAddress> Addresses { get; set; } = new List<IAddress>();
        [Parameter] public bool CustomerEditApprovalRequired { get; set; }
        [Parameter] public string LinkedItemUID { get; set; }
        [Parameter] public string NewlyBillToAddressUID { get; set; }
        [Parameter] public bool IsEditOnBoardDetails { get; set; } = false;
        [Parameter] public string StoreUID { get; set; } = "";
        public bool IsShowAllBillToAddress { get; set; } = false;
        private bool IsEditPage = false;
        private bool IsInitialised { get; set; } = false;
        public string ButtonName { get; set; } = "Save";
        public bool IsSuccess { get; set; } = false;

        [Parameter] public bool IsBackBtnClicked { get; set; } = false;
        public List<IAddress> duplicateAddress { get; set; } = new List<IAddress>();
        [Parameter] public GSTINDetailsModel BillToAddressFromGst { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            // IsSuccess = true;
            SelectedAddress = _serviceProvider.CreateInstance<IAddress>();
            SelectedAddress.SectionName = OnboardingScreenConstant.BilltoAddress;
            GSTINDetailsModel.PR_ADR_Pincode = SelectedAddress.ZipCode;
            duplicateAddress = Addresses.Select(address => (address as Winit.Modules.Address.Model.Classes.Address)?.DeepCopy() as IAddress).ToList();
            SelectedAddress.SectionName = OnboardingScreenConstant.BilltoAddress;
            _locationInformation = _serviceProvider.CreateInstance<IAddress>();
            SelectedLocation = _serviceProvider.CreateInstance<Winit.Modules.Location.Model.Interfaces.ILocationData>();
            //SelectedAddress.IsDefault = true;
            isChecked = true;
            await CheckboxChanged();
            GenderselectionItems = new List<ISelectionItem>
        {
            new SelectionItem { UID = "1", Label = "Male"},
            new SelectionItem { UID = "2", Label = "Female"},
            new SelectionItem { UID = "3", Label = "Other" },
        };
            Addresses.Clear();
            Addresses.AddRange(await OnShowAllBilltoAddressClick.Invoke());
            GenerateGridColumns();
            if (TabName == StoreConstants.Confirmed)
            {
                var concreteAddress = SelectedAddress as Winit.Modules.Address.Model.Classes.Address;
                OriginalSelectedAddress = concreteAddress.DeepCopy();// Create a deep copy
            }
            IsInitialised = true;
            StateHasChanged();
            await Task.CompletedTask;
        }
        //public void OnGenderSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        //{
        //    SelectedAddress.Gender = null;
        //    if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        //    {
        //        var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
        //        // sku.Code = selecetedValue?.Code;
        //        SelectedAddress.Gender= selecetedValue?.Label;
        //    }
        //}
        private bool isChecked;
        private bool IsChecked
        {
            get => isChecked;
            set
            {
                if (isChecked != value)
                {
                    isChecked = value;
                    _ = HandleCheckboxChangedAsync();
                }
            }

        }
        private async Task HandleCheckboxChangedAsync()
        {
            await CheckboxChanged();
        }
        public void PopulateAddresses()
        {
            try
            {
                const int maxLength = 100;

                if (!string.IsNullOrEmpty(StoreAdditionalInfo.GSTAddress1))
                {
                    if (StoreAdditionalInfo.GSTAddress1.Length > maxLength)
                    {
                        SelectedAddress.Line1 = StoreAdditionalInfo.GSTAddress1.Substring(0, maxLength);

                        if (!string.IsNullOrEmpty(StoreAdditionalInfo.GSTAddress2))
                        {
                            SelectedAddress.Line2 = StoreAdditionalInfo.GSTAddress1.Substring(maxLength) + " " + (StoreAdditionalInfo.GSTAddress2.Length > maxLength
                                ? StoreAdditionalInfo.GSTAddress2.Substring(0, maxLength)
                                : StoreAdditionalInfo.GSTAddress2);
                        }
                        else
                        {
                            SelectedAddress.Line2 = StoreAdditionalInfo.GSTAddress1.Substring(maxLength);
                        }
                    }
                    else
                    {
                        SelectedAddress.Line1 = StoreAdditionalInfo.GSTAddress1;

                        if (!string.IsNullOrEmpty(StoreAdditionalInfo.GSTAddress2))
                        {
                            SelectedAddress.Line2 = StoreAdditionalInfo.GSTAddress2.Length > maxLength
                                ? StoreAdditionalInfo.GSTAddress2.Substring(0, maxLength)
                                : StoreAdditionalInfo.GSTAddress2;
                        }
                        else
                        {
                            SelectedAddress.Line2 = string.Empty;
                        }
                    }
                }
                else
                {
                    // Handle the case when GSTAddress1 is null or empty
                    SelectedAddress.Line1 = string.Empty;

                    if (!string.IsNullOrEmpty(StoreAdditionalInfo.GSTAddress2))
                    {
                        SelectedAddress.Line2 = StoreAdditionalInfo.GSTAddress2.Length > maxLength
                            ? StoreAdditionalInfo.GSTAddress2.Substring(0, maxLength)
                            : StoreAdditionalInfo.GSTAddress2;
                    }
                    else
                    {
                        SelectedAddress.Line2 = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                StateHasChanged();
            }
        }
        public async Task PopulateLocationDropDowns(GSTINDetailsModel gSTINDetailsModel)
        {
            try
            {
                if (IsEditOnBoardDetails)
                {
                    gSTINDetailsModel = new GSTINDetailsModel
                    {
                        PR_ADR_State = StoreAdditionalInfo.GSTState ?? "",
                        PR_ADR_District = StoreAdditionalInfo.GSTDistrict ?? "",
                        PR_ADR_Pincode = StoreAdditionalInfo.PinCode ?? "",
                    };
                }
                await OnCopyGSTLocationDropdowns.InvokeAsync(gSTINDetailsModel);
                _loadingService.HideLoading();
                AssignFieldvalues(gSTINDetailsModel);
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public void AssignFieldvalues(GSTINDetailsModel gSTINDetailsModel)
        {
            try
            {
                SelectedAddress.State = StateselectionItems.Any(p => p.IsSelected) ? gSTINDetailsModel.PR_ADR_State ?? string.Empty : string.Empty;
                SelectedAddress.City = CityselectionItems.Any(p => p.IsSelected) ? gSTINDetailsModel.PR_ADR_District ?? string.Empty : string.Empty;
                SelectedAddress.ZipCode = PinCodeselectionItems.Any(p => p.IsSelected) ? gSTINDetailsModel.PR_ADR_Pincode ?? string.Empty : string.Empty;
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        private async Task CheckboxChanged()
        {
            try
            {
                if (IsChecked)
                {
                    // Filter out null or empty values and join the remaining parts with a comma
                    SelectedAddress.Line1 = StoreAdditionalInfo.GSTAddress;
                    PopulateAddresses();
                    PopulateLocationDropDowns(GSTINDetailsModel);
                    //SelectedAddress.Name = GSTINDetailsModel.LegalName;
                    //SelectedAddress.ZipCode = GSTINDetailsModel.PR_ADR_Pincode;
                    // SelectedAddress.State = GSTINDetailsModel.PR_ADR_State;
                    SelectedAddress.Name = GSTINDetailsModel.LegalName;
                    SelectedAddress.GSTNo = GSTINDetailsModel.GSTIN;
                }
                else
                {
                    DeselectLocationDropDowns();
                    SelectedAddress.Line1 = string.Empty;
                    SelectedAddress.Line2 = string.Empty;
                    // SelectedAddress.Name = string.Empty; 
                    SelectedAddress.ZipCode = string.Empty;
                    SelectedAddress.State = string.Empty;
                    SelectedAddress.City = string.Empty;
                    SelectedAddress.Name = string.Empty;
                    SelectedAddress.GSTNo = string.Empty;
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                StateHasChanged();
            }
        }
        private string AadharNumbervalidationMessage = string.Empty;
        private void ValidateaadharNumber(ChangeEventArgs e)
        {
            string input = e.Value?.ToString();

            if (string.IsNullOrEmpty(input) || input.Length != 12)
            {
                AadharNumbervalidationMessage = "Aadhar number should be 12 digits.";
            }
            else
            {
                AadharNumbervalidationMessage = string.Empty;
            }
        }
        public bool IsValidAadharNumber(string AadharNumber)
        {
            if (string.IsNullOrWhiteSpace(AadharNumber))
            {
                return false;
            }

            // Example: Account number validation for numbers between 8 and 10 digits
            var accountPattern = @"^\d{12}$";
            return Regex.IsMatch(AadharNumber, accountPattern);
        }
        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {

                new DataGridColumn {Header = "Address Person Name", GetValue = s => ((IAddress)s)?.Name ?? "N/A" },
                new DataGridColumn {Header = "Email", GetValue = s => ((IAddress)s)?.Email ?? "N/A"},
                new DataGridColumn {Header = "Mobile Number 1", GetValue = s => ((IAddress)s)?.Mobile1 ?? "N/A"},
                new DataGridColumn {Header = "Mobile Number 2", GetValue = s => ((IAddress)s)?.Mobile2 ?? "N/A"},
              //  new DataGridColumn {Header = "Sales Office", GetValue = s => ((IAddress)s)?.SalesOfficeUID ?? "N/A"},
                new DataGridColumn {Header = "City", GetValue = s => ((IAddress)s)?.City ?? "N/A"},
                new DataGridColumn {Header = "Address 1", GetValue = s => ((IAddress)s)?.Line1 ?? "N/A"},
                new DataGridColumn {Header = "Address 2", GetValue = s => ((IAddress)s)?.Line2 ?? "N/A"},
                new DataGridColumn {Header = "Is Primary", GetValue = s => ((IAddress)s)?.IsDefault.ToString() ?? "N/A"},
                new DataGridColumn
                {
                Header = "Actions",
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    new ButtonAction
                    {
                        ButtonType = ButtonTypes.Image,
                        URL = "Images/edit.png",
                        Action = item => OnEditClick((IAddress)item)

                    },
                    new ButtonAction
                    {
                        ButtonType = ButtonTypes.Image,
                        URL = "Images/delete.png",
                        Action = item => OnDeleteClick((IAddress)item)

                    }
                }
            }
             };
        }
        public void OnEditClick(IAddress address)
        {
            OriginalSelectedAddress = address;
            IAddress duplicateaddress = (address as Winit.Modules.Address.Model.Classes.Address).DeepCopy()!;
            //SelectedAddress.ZipCode= GSTINDetailsModel.PR_ADR_Pincode;
            SelectedAddress = duplicateaddress;
            IsEditPage = true;
            ButtonName = "Update";
            SetEditForGenderDD(SelectedAddress);
            FocusOnContactName();
            OnEditInvokeLocationDropdowns.InvokeAsync(address);
            StateHasChanged();
        }
        public void AssignOU(IAddress address)
        {
            try
            {
                var selectedOUShipTo = OUselectionItems.Find(e => e.Code == address.OrgUnitUID);
                if (selectedOUShipTo != null)
                {
                    SelectedAddress.OrgUnitUID = selectedOUShipTo.UID;
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void SetEditForGenderDD(IAddress address)
        {

            foreach (var item in GenderselectionItems)
            {
                if (item.Label == address.Line4)
                {
                    item.IsSelected = true;
                }
                else
                {
                    item.IsSelected = false;
                }
            }
        }
        public async Task OnDeleteClick(IAddress address)
        {
            await OnDelete.InvokeAsync(address.UID);
            IsShowAllBillToAddress = false;
            await OnClean();
        }
        private void DeselectGender()
        {
            if (GenderselectionItems != null)
            {
                foreach (var item in GenderselectionItems)
                {
                    item.IsSelected = false;
                }
            }
        }
        private void DeselectLocationDropDowns()
        {
            try
            {
                StateselectionItems.ForEach(p => p.IsSelected = false);
                CityselectionItems.ForEach(p => p.IsSelected = false);
                LocalityselectionItems.ForEach(p => p.IsSelected = false);
                BranchselectionItems.ForEach(p => p.IsSelected = false);
                PinCodeselectionItems.ForEach(p => p.IsSelected = false);
                OUselectionItems.ForEach(p => p.IsSelected = false);
                ASMselectionItems.ForEach(p => p.IsSelected = false);
                SalesOfficeselectionItems.ForEach(p => p.IsSelected = false);
            }
            catch (Exception ex)
            {

            }

        }
        protected async Task OnClean()
        {
            DeselectGender();
            DeselectLocationDropDowns();
            SelectedAddress = ResetAddress(SelectedAddress);
            if (TabName == StoreConstants.Confirmed)
            {
                OriginalSelectedAddress = ResetAddress(OriginalSelectedAddress);
            }
            GSTINDetailsModel.PR_ADR_Pincode = string.Empty;
            IsEditPage = false;
            ButtonName = "Save";
            StateHasChanged();
        }


        // New method to reset the Address object
        private Winit.Modules.Address.Model.Classes.Address ResetAddress(Winit.Modules.Address.Model.Interfaces.IAddress address)
        {
            return new Winit.Modules.Address.Model.Classes.Address
            {
                Name = string.Empty,
                HusbandName = string.Empty,
                FatherName = string.Empty,
                Line4 = string.Empty,
                Mobile1 = string.Empty,
                AADHAR = string.Empty,
                Phone = string.Empty,
                GSTNo = string.Empty,
                MuncipalRegNo = string.Empty,
                PFRegNo = string.Empty,
                ESICRegNo = string.Empty,
                PhoneExtension = string.Empty,
                Email = string.Empty,
                State = string.Empty,
                IsDefault = false
            };
        }

        public async Task GetAllBillToAddress()
        {
            IsShowAllBillToAddress = !IsShowAllBillToAddress;
            _loadingService.ShowLoading();
            if (!string.IsNullOrEmpty(StoreUID))
            {
                Addresses.Clear();
                Addresses.AddRange(await OnShowAllBilltoAddressClick.Invoke());
            }
            StateHasChanged();
            _loadingService.HideLoading();
        }
        private bool IsValidMobileNumber(string mobileNumber)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
            {
                return false;
            }
            // Example: Mobile number validation for 10-digit numbers starting with a digit between 6-9
            var mobilePattern = @"^[6-9]\d{9}$";
            return Regex.IsMatch(mobileNumber, mobilePattern);
        }
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, emailPattern);
        }
        private string MobilevalidationMessage = string.Empty;

        private void ValidateMobileNumber(ChangeEventArgs e)
        {
            string input = e.Value?.ToString();

            if (string.IsNullOrEmpty(input) || input.Length != 10)
            {
                MobilevalidationMessage = "Mobile number must be exactly 10 digits.";
            }
            else
            {
                MobilevalidationMessage = string.Empty;
            }
        }
        public async Task<bool> ValidateFields()
        {
            IsSaveAttempted = true;
            ValidationMessage = null;

            if (string.IsNullOrWhiteSpace(SelectedAddress.Name) ||
               string.IsNullOrWhiteSpace(SelectedAddress.Line1) ||
                !IsValidMobileNumber(SelectedAddress.Mobile1) ||
                !IsValidAadharNumber(SelectedAddress.AADHAR) ||
                  !IsValidPinCode(SelectedAddress.ZipCode) ||
                !IsValidEmail(SelectedAddress.Email) ||
                string.IsNullOrWhiteSpace(SelectedAddress.State) ||
                string.IsNullOrWhiteSpace(SelectedAddress.City) ||
                string.IsNullOrWhiteSpace(SelectedAddress.Locality) ||
                string.IsNullOrWhiteSpace(SelectedAddress.BranchUID) ||
                string.IsNullOrWhiteSpace(SelectedAddress.ZipCode) || string.IsNullOrWhiteSpace(SelectedAddress.OrgUnitUID))

            {
                ValidationMessage = "The following fields have invalid field(s)" + ": ";

                if (string.IsNullOrWhiteSpace(SelectedAddress.Name))
                {
                    ValidationMessage += "Name, ";
                }
                if (string.IsNullOrWhiteSpace(SelectedAddress.Line1))
                {
                    ValidationMessage += "Address1, ";
                }
                if (!IsValidPinCode(SelectedAddress.ZipCode) || string.IsNullOrWhiteSpace(SelectedAddress.ZipCode))
                {
                    ValidationMessage += "PinCode, ";
                }

                if (string.IsNullOrWhiteSpace(SelectedAddress.Mobile1) || !IsValidMobileNumber(SelectedAddress.Mobile1))
                {
                    ValidationMessage += "Mobile Number, ";
                }
                if (string.IsNullOrWhiteSpace(SelectedAddress.Email) || !IsValidEmail(SelectedAddress.Email))
                {
                    ValidationMessage += "Email, ";
                }

                if (string.IsNullOrWhiteSpace(SelectedAddress.AADHAR) || !IsValidAadharNumber(SelectedAddress.AADHAR))
                {
                    ValidationMessage += "Aadhar Number, ";
                }
                if (string.IsNullOrWhiteSpace(SelectedAddress.State))
                {
                    ValidationMessage += "State, ";
                }
                if (string.IsNullOrWhiteSpace(SelectedAddress.City))
                {
                    ValidationMessage += "City, ";
                }
                if (string.IsNullOrWhiteSpace(SelectedAddress.Locality))
                {
                    ValidationMessage += "Locality, ";
                }
                if (string.IsNullOrWhiteSpace(SelectedAddress.BranchUID))
                {
                    ValidationMessage += "Branch, ";
                }
                if (string.IsNullOrWhiteSpace(SelectedAddress.OrgUnitUID))
                {
                    ValidationMessage += "Organisation Unit, ";
                }
                //if (string.IsNullOrWhiteSpace(SelfRegistrationUID))
                //{
                //    if (string.IsNullOrWhiteSpace(SelectedAddress.ASMEmpUID))
                //    {
                //        ValidationMessage += "Asm, ";
                //    }
                //    if (string.IsNullOrWhiteSpace(SelectedAddress.SalesOfficeUID))
                //    {
                //        ValidationMessage += "SalesOffice, ";
                //    }
                //}

                ValidationMessage = ValidationMessage.TrimEnd(' ', ',');
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task SaveOrUpdate()
        {
            if (!IsBackBtnClicked)
            {
                //ValidateFields();
            }
            try
            {
                if (!await ValidateFields())
                {
                    if (!IsEditPage)
                    {
                        await OnAddBilltoAddress.InvokeAsync();

                    }
                    else
                    {
                        await OnEditBilltoAddress.InvokeAsync();
                        if (IsEditPage)
                        {
                            IsShowAllBillToAddress = !IsShowAllBillToAddress;
                        }
                    }
                    string actionType = SelectedAddress.UID == null ? OnboardingScreenConstant.Create : OnboardingScreenConstant.Update;
                    if (ButtonName == "Update")
                    {
                        AssignOU(SelectedAddress);
                    }
                    if (TabName == StoreConstants.Confirmed && !CustomerEditApprovalRequired)
                    {
                        await SaveOrUpdateBilltoAddress.InvokeAsync(SelectedAddress);
                        isChecked = false;
                        await RequestChange(actionType);
                    }
                    else if (TabName == StoreConstants.Confirmed)
                    {
                        await RequestChange(actionType);
                    }
                    else
                    {
                        await SaveOrUpdateBilltoAddress.InvokeAsync(SelectedAddress);
                        isChecked = false;
                    }
                    //DeselectLocationDropDowns();
                    if (IsBillToAddressSuccess)
                    {
                        await OnClean();
                    }
                    // GenerateGridColumns();
                    Addresses.Clear();
                    Addresses.AddRange(await OnShowAllBilltoAddressClick.Invoke());
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private bool IsValidPinCode(string pinCode)
        {
            if (string.IsNullOrWhiteSpace(pinCode))
            {
                return false;
            }

            // Example: PIN code validation for 6-digit numbers
            var pinCodePattern = @"^\d{6}$";
            return Regex.IsMatch(pinCode, pinCodePattern);
        }
        //private void ValidateAllFields()
        //{
        //    ValidationMessage = null;

        //    if (string.IsNullOrWhiteSpace(SelectedAddress.Name) ||
        //        string.IsNullOrWhiteSpace(SelectedAddress.Phone) ||
        //        string.IsNullOrWhiteSpace(SelectedAddress.AADHAR))

        //    {
        //        ValidationMessage = "The following fields have invalid field(s)" + ": ";

        //        if (string.IsNullOrWhiteSpace(SelectedAddress.Name))
        //        {
        //            ValidationMessage += " Name, ";
        //        }
        //        if (string.IsNullOrWhiteSpace(SelectedAddress.Phone))
        //        {
        //            ValidationMessage += "Mobile Number, ";
        //        }
        //        if (string.IsNullOrWhiteSpace(SelectedAddress.AADHAR))
        //        {
        //            ValidationMessage += "AAdhar Number, ";
        //        }
        //        ValidationMessage = ValidationMessage.TrimEnd(' ', ',');
        //    }
        //}
        //private void OnLocationChange(Winit.Modules.Location.Model.Interfaces.ILocationData locationMasterForUI)
        //{
        //    ViewLocation = false;
        //    if (locationMasterForUI == null)
        //    {
        //        _locationInformation.LocationLabel = $"Select {_appSettings.LocationLevel}";
        //        _locationInformation.LocationUID = string.Empty;
        //        LocationMasters = null;
        //    }
        //    else
        //    {
        //        SelectedLocation = locationMasterForUI;
        //        _locationInformation.LocationLabel = locationMasterForUI.PrimaryLabel;
        //        _locationInformation.LocationUID = locationMasterForUI.PrimaryUid;
        //        _locationInformation.LocationJson = locationMasterForUI.JsonData;
        //        DeserializeJsonLocation();
        //    }
        //}
        //protected void DeserializeJsonLocation()
        //{
        //    LocationMasters = JsonConvert.DeserializeObject<List<Winit.Modules.Location.Model.Classes.LocationMaster>>(_locationInformation.LocationJson);
        //    if (LocationMasters != null)
        //    {
        //        LocationMasters = LocationMasters.OrderBy(p => p.Level).ToList();
        //    }
        //}
        public async Task GenderSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            SelectedAddress.Line4 = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                // sku.Code = selecetedValue?.Code;
                SelectedAddress.Line4 = selecetedValue?.Label;
                StateHasChanged();
            }
        }
        public async Task StateSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            //SelectedAddress.Line4 = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                SelectedAddress.State = selecetedValue?.Code;
                SelectedAddress.City = string.Empty;
                SelectedAddress.Locality = string.Empty;
                SelectedAddress.ZipCode = string.Empty;
                SelectedAddress.BranchUID = string.Empty;
                SelectedAddress.OrgUnitUID = string.Empty;
                await StateSelected.InvokeAsync(new List<string> { selecetedValue.UID });
            }
            else
            {
                SelectedAddress.State = string.Empty;
            }
            StateHasChanged();
        }
        public async Task CitySelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            //SelectedAddress.Line4 = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                SelectedAddress.City = selecetedValue?.Code;
                SelectedAddress.Locality = string.Empty;
                SelectedAddress.ZipCode = string.Empty;
                SelectedAddress.BranchUID = string.Empty;
                SelectedAddress.OrgUnitUID = string.Empty;
                await CitySelected.InvokeAsync(new List<string> { selecetedValue.UID });
            }
            else
            {
                SelectedAddress.City = string.Empty;
            }
            StateHasChanged();
        }
        public async Task LocalitySelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            //SelectedAddress.Line4 = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                SelectedAddress.Locality = selecetedValue?.Code;
                SelectedAddress.ZipCode = string.Empty;
                SelectedAddress.BranchUID = string.Empty;
                await LocalitySelected.InvokeAsync(new List<string> { selecetedValue.UID });
            }
            else
            {
                SelectedAddress.Locality = string.Empty;
            }
            StateHasChanged();
        }
        public async Task PinCodeSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            //SelectedAddress.Line4 = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                SelectedAddress.ZipCode = selecetedValue?.Code;
            }
            else
            {
                SelectedAddress.ZipCode = string.Empty;
            }
            StateHasChanged();
        }
        public async Task BranchSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            //SelectedAddress.Line4 = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                SelectedAddress.BranchUID = selecetedValue?.UID;
                await BranchSelected.InvokeAsync(selecetedValue.UID);
            }
            else
            {
                SelectedAddress.BranchUID = string.Empty;
            }
            StateHasChanged();
        }
        public async Task OUSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            //SelectedAddress.Line4 = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                SelectedAddress.OrgUnitUID = selecetedValue?.UID;
            }
            else
            {
                SelectedAddress.OrgUnitUID = string.Empty;
            }
            StateHasChanged();
        }
        //public async Task ASMSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        //{
        //    //SelectedAddress.Line4 = null;
        //    if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        //    {
        //        var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
        //        SelectedAddress.ASMEmpUID = selecetedValue?.Label;
        //        StateHasChanged();
        //    }
        //}
        public async Task SalesOfficeSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            //SelectedAddress.Line4 = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                SelectedAddress.SalesOfficeUID = selecetedValue?.UID;
            }
            else
            {
                SelectedAddress.SalesOfficeUID = string.Empty;
            }
            StateHasChanged();
        }
        private ElementReference BillToAddressInput;
        private async void FocusOnContactName()
        {
            await BillToAddressInput.FocusAsync();
        }
        public bool AreBillToAddressEqual()
        {
            // Check if the duplicateContact list is null or empty
            if (duplicateAddress == null || !duplicateAddress.Any())
            {
                return false;
            }

            // Loop through each contact in the duplicateContact list
            foreach (var address in duplicateAddress)
            {
                // Compare SelectedContact with each contact in the duplicateContact list
                if (
                    SelectedAddress.Name == address.Name ||
                    SelectedAddress.Email == address.Email ||
                    SelectedAddress.Phone == address.Phone ||
                    SelectedAddress.Line1 == address.Line1 ||
                    SelectedAddress.Line2 == address.Line2 ||
                    SelectedAddress.Line3 == address.Line3 ||
                    SelectedAddress.Line4 == address.Line4 ||
                    SelectedAddress.FatherName == address.FatherName ||
                    SelectedAddress.HusbandName == address.HusbandName ||
                    SelectedAddress.ZipCode == address.ZipCode ||
                    SelectedAddress.AADHAR == address.AADHAR ||
                    SelectedAddress.GSTNo == address.GSTNo ||
                    SelectedAddress.Mobile1 == address.Mobile1 ||
                    SelectedAddress.Mobile2 == address.Mobile2 ||
                    SelectedAddress.MuncipalRegNo == address.MuncipalRegNo ||
                    SelectedAddress.ESICRegNo == address.ESICRegNo ||
                    SelectedAddress.PFRegNo == address.PFRegNo ||
                    SelectedAddress.IsDefault == address.IsDefault)
                {
                    return true;
                }
            }

            return false; // Return false if no matching contact is found
        }
        #region Change RequestLogic

        public async Task RequestChange(string actionType)
        {
            SelectedAddress.Type = OnboardingScreenConstant.Billing;
            SelectedAddress.LinkedItemType = OnboardingScreenConstant.LinkedItemTypeStore;

            // Directly creating List<IChangeRecordDTO> using inline creation of ChangeRecordDTOs
            List<IChangeRecordDTO> ChangeRecordDTOs = new List<IChangeRecordDTO>
            {
                new ChangeRecordDTO
                {
                    LinkedItemUID=LinkedItemUID,
                    Action= actionType,
                    ScreenModelName = OnboardingScreenConstant.BilltoAddress,
                    UID = SelectedAddress.UID==null?(NewlyBillToAddressUID==null?Guid.NewGuid().ToString():NewlyBillToAddressUID):OriginalSelectedAddress?.UID!,
                    ChangeRecords = CommonFunctions.GetChangedData(CommonFunctions.CompareObjects(OriginalSelectedAddress, SelectedAddress)!)
                }
            }
            .Where(c => c.ChangeRecords.Count > 0)
            .ToList();

            if (ChangeRecordDTOs.Count > 0)
            {
                var ChangeRecordDTOInJson = CommonFunctions.ConvertToJson(ChangeRecordDTOs);
                await InsertDataInChangeRequest.InvokeAsync(ChangeRecordDTOInJson);
                OriginalSelectedAddress = ResetAddress(OriginalSelectedAddress);
                SelectedAddress = ResetAddress(SelectedAddress);

            }
            ChangeRecordDTOs.Clear();
        }
        #endregion
    }
}
