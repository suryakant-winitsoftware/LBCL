using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;
using Winit.Modules.Address.Model.Classes;
using Winit.Modules.Address.Model.Interfaces;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Constants;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.UIModels.Common.GST;
using WINITMobile.Pages.Base;

namespace WINITMobile.Pages.Customer_Details
{
    public partial class ShipToAddress : BaseComponentBase
    {
        [Parameter] public IAddress _locationInformation { get; set; }
        protected List<Winit.Modules.Location.Model.Classes.LocationMaster>? LocationMasters;
        [Inject] Winit.Modules.Setting.BL.Interfaces.IAppSetting _appSettings { get; set; }
        public Winit.Modules.Location.Model.Interfaces.ILocationData SelectedLocation { get; set; }
        private bool ViewLocation { get; set; }

        public List<ISelectionItem> selectionItems = new();
        public string ValidationMessage;
        private bool IsSaveAttempted { get; set; } = false;
        public List<DataGridColumn> DataGridColumns { get; set; }
        public List<DataGridColumn> DataGridColumnsForAsm { get; set; }
        public List<DataGridColumn> DataGridColumnsForAsmPopUp { get; set; }
        public IAddress SelectedAddress { get; set; } = new Address();
        public IAddress OriginalSelectedAddress { get; set; } = new Address();
        [Parameter] public string TabName { get; set; }
        [Parameter] public IOnBoardEditCustomerDTO EditOnBoardingDetails { get; set; }
        [Parameter] public EventCallback<IAddress> SaveOrUpdateAddress { get; set; }
        [Parameter] public EventCallback<List<IAsmDivisionMapping>> SaveOrUpdateAsmDivision { get; set; }
        [Parameter] public EventCallback<IAsmDivisionMapping> DeleteAsmDivision { get; set; }
        [Parameter] public EventCallback<IAddress> OnAddAddress { get; set; }
        [Parameter] public EventCallback<IAddress> OnEditAddress { get; set; }
        [Parameter] public EventCallback<string> OnDelete { get; set; }
        [Parameter] public bool IsBillToAddressSuccess { get; set; }
        [Parameter] public Func<Task<List<IAddress>>> OnShowAllAddresssClick { get; set; }
        [Parameter] public List<IAddress> Addresses { get; set; } = new List<IAddress>();
        [Parameter] public List<IAddress> BillToAddresses { get; set; } = new List<IAddress>();
        [Parameter] public List<ISelectionItem> StateselectionItems { get; set; }
        [Parameter] public List<ISelectionItem> CityselectionItems { get; set; }
        [Parameter] public List<ISelectionItem> LocalityselectionItems { get; set; }
        [Parameter] public List<ISelectionItem> BranchselectionItems { get; set; }
        [Parameter] public List<ISelectionItem> ASMselectionItems { get; set; }
        [Parameter] public List<ISelectionItem> ASEMselectionItems { get; set; }
        [Parameter] public List<ISelectionItem> DivisionselectionItems { get; set; }
        [Parameter] public List<ISelectionItem> OUselectionItemsForShipTo { get; set; }
        [Parameter] public List<ISelectionItem> SalesOfficeselectionItems { get; set; }
        [Parameter] public List<ISelectionItem> PinCodeselectionItems { get; set; }
        [Parameter] public EventCallback<List<string>> StateSelected { get; set; }
        [Parameter] public EventCallback<List<string>> CitySelected { get; set; }
        [Parameter] public EventCallback<List<string>> LocalitySelected { get; set; }
        [Parameter] public EventCallback<string> BranchSelected { get; set; }
        [Parameter] public EventCallback<string> ASMDivisionInvoke { get; set; }
        [Parameter] public EventCallback<string> OUSelected { get; set; }
        [Parameter] public string SelfRegistrationUID { get; set; }
        [Parameter] public string BroadClassification { get; set; }
        [Parameter] public EventCallback<string> InsertDataInChangeRequest { get; set; }
        [Parameter] public EventCallback<IAddress> OnEditClickInvokeAddressDropdowns { get; set; }
        [Parameter] public EventCallback<IAddress> OnCopyInvokeAddressDropdowns { get; set; }
        [Parameter] public EventCallback OnLoad { get; set; }
        [Parameter] public bool CustomerEditApprovalRequired { get; set; }
        [Parameter] public string LinkedItemUID { get; set; }

        private bool IsEditPage = false;
        public bool IsSuccess { get; set; } = false;
        [Parameter] public bool IsMultipleAsmAllowed { get; set; }
        [Parameter] public bool IsAsmMappedByCustomer { get; set; }
        [Parameter] public List<GSTINDetailsModel> ShipToAddressFromGst { get; set; }
        public bool IsShowAllShipToAddress { get; set; } = false;
        private bool IsInitialised { get; set; } = false;
        private bool IsAsmAdd { get; set; } = false;
        public string ButtonName { get; set; } = "Save";
        [Parameter] public bool IsBackBtnClicked { get; set; } = false;
        public List<IAsmDivisionMapping> AsmDivisionDetails { get; set; } = new List<IAsmDivisionMapping>();
        public List<IAsmDivisionMapping> AsmDivisionDetailsDB { get; set; } = new List<IAsmDivisionMapping>();
        [Parameter] public List<IAsmDivisionMapping> AsmDivisionList { get; set; } = new List<IAsmDivisionMapping>();
        IAsmDivisionMapping obj { get; set; } = new AsmDivisionMapping();
        IAsmDivisionMapping objCopy { get; set; } = new AsmDivisionMapping();
        public string ErrorMessage { get; set; } = "No address is marked as primary. Please mark at least one address as primary.";
        [Parameter] public string StoreUID { get; set; } = "";
        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            IsSuccess = true;
            SelectedAddress = _serviceProvider.CreateInstance<IAddress>();
            _locationInformation = _serviceProvider.CreateInstance<IAddress>();
            SelectedLocation = _serviceProvider.CreateInstance<ILocationData>();
            SelectedAddress.IsDefault = true;
            Addresses.Clear();
            Addresses.AddRange(await OnShowAllAddresssClick.Invoke());
            await GenerateGridColumns();
            await GenerateAsmGridColumns();
            IsInitialised = true;
            if (TabName == StoreConstants.Confirmed)
            {
                var concreteAddress = SelectedAddress as Winit.Modules.Address.Model.Classes.Address;
                OriginalSelectedAddress = concreteAddress.DeepCopy();// Create a deep copy
            }
            await Task.CompletedTask;
            await OnLoad.InvokeAsync();
            _loadingService.HideLoading();
        }
        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {

               // new DataGridColumn {Header = "Address Person Name", GetValue = s => ((IAddress)s)?.Name ?? "N/A" },
                new DataGridColumn {Header = "Email", GetValue = s => ((IAddress)s)?.Email ?? "N/A"},
                new DataGridColumn {Header = "Mobile Number", GetValue = s => ((IAddress)s)?.Mobile1 ?? "N/A"},
                new DataGridColumn {Header = "Organisation Unit", GetValue = s => ((IAddress)s)?.OrgUnitUID ?? "N/A"},
                new DataGridColumn {Header = "Site No", GetValue = s => ((IAddress)s)?.Mobile1 ?? "N/A"},
                new DataGridColumn {Header = "State", GetValue = s => ((IAddress)s)?.State ?? "N/A"},
                new DataGridColumn {Header = "Branch", GetValue = s => ((IAddress)s)?.BranchUID ?? "N/A"},
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
        private async Task GenerateAsmGridColumns()
        {
            DataGridColumnsForAsm = new List<DataGridColumn>
            {

               // new DataGridColumn {Header = "Address Person Name", GetValue = s => ((IAddress)s)?.Name ?? "N/A" },
                new DataGridColumn {Header = "Division", GetValue = s => ((IAsmDivisionMapping)s)?.DivisionName ?? "N/A"},
                new DataGridColumn {Header = "ASM", GetValue = s => ((IAsmDivisionMapping)s)?.AsmEmpName ?? "N/A"},
                new DataGridColumn
                {
                    Header = "Actions",
                    IsButtonColumn = true,
                    ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction
                        {
                            ButtonType = ButtonTypes.Image,
                            URL = "Images/delete.png",
                            Action = item => OnDeleteClickAsm((IAsmDivisionMapping)item)

                        }
                    }
                }
            };
            DataGridColumnsForAsmPopUp = new List<DataGridColumn>
            {

               // new DataGridColumn {Header = "Address Person Name", GetValue = s => ((IAddress)s)?.Name ?? "N/A" },
                new DataGridColumn {Header = "Division", GetValue = s => ((IAsmDivisionMapping)s)?.DivisionName ?? "N/A"},
                new DataGridColumn {Header = "ASM", GetValue = s => ((IAsmDivisionMapping)s)?.AsmEmpName ?? "N/A"},
            };
        }
        public void OnEditClick(IAddress address)
        {

            if (!string.IsNullOrEmpty(address.UID))
            {
                OriginalSelectedAddress = address;
                IAddress duplicateaddress = (address as Address).DeepCopy()!;
                SelectedAddress = duplicateaddress;
                IsEditPage = true;
                ButtonName = "Update";
                ASMDivisionInvoke.InvokeAsync(address.UID);
                OnEditClickInvokeAddressDropdowns.InvokeAsync(address);
                StateHasChanged();
            }
            else
            {
                ErrorMessage = "Please fill Customer Information.";
                ShowNoPrimaryAddressPopup();
                StateHasChanged();
            }
        }
        public async Task OnDeleteClick(IAddress address)
        {
            await OnDelete.InvokeAsync(address.UID);


            IsShowAllShipToAddress = false;
            await OnClean();

        }
        public async Task OnDeleteClickAsm(IAsmDivisionMapping asmDivision)
        {
            bool AsmDelete = await _alertService.ShowConfirmationReturnType("Alert", "Are you sure you want to Delete this item?", "Yes", "No");
            if (AsmDelete)
            {
                AsmDivisionDetails.Remove(asmDivision);
                DeleteAsmDivision.InvokeAsync(asmDivision);
            }
            StateHasChanged();
        }
        private void DeselectLocationDropDowns()
        {
            StateselectionItems.ForEach(p => p.IsSelected = false);
            CityselectionItems.ForEach(p => p.IsSelected = false);
            LocalityselectionItems.ForEach(p => p.IsSelected = false);
            BranchselectionItems.ForEach(p => p.IsSelected = false);
            PinCodeselectionItems.ForEach(p => p.IsSelected = false);
            DivisionselectionItems.ForEach(p => p.IsSelected = false);
            OUselectionItemsForShipTo.ForEach(p => p.IsSelected = false);
            ASMselectionItems.ForEach(p => p.IsSelected = false);
            //ASEMselectionItems.ForEach(p => p.IsSelected = false);
            SalesOfficeselectionItems.ForEach(p => p.IsSelected = false);
        }
        protected async Task OnClean()
        {
            DeselectLocationDropDowns();
            AsmDivisionDetails.Clear();
            SelectedAddress = ResetAddress();
            if (TabName == StoreConstants.Confirmed)
            {
                OriginalSelectedAddress = ResetAddress();
            }

            IsEditPage = false;
            ButtonName = "Save";
            StateHasChanged();
        }
        private Winit.Modules.Address.Model.Classes.Address ResetAddress()
        {
            return new Winit.Modules.Address.Model.Classes.Address
            {
                Landmark = string.Empty,
                ZipCode = string.Empty,
                Mobile1 = string.Empty,
                PhoneExtension = string.Empty,
                Email = string.Empty,
                IsDefault = false
            };
        }
        public async Task GetAllShipToAddress()
        {
            IsShowAllShipToAddress = !IsShowAllShipToAddress;
            _loadingService.ShowLoading();
            if (!string.IsNullOrEmpty(StoreUID))
            {
                Addresses.Clear();
                Addresses.AddRange(await OnShowAllAddresssClick.Invoke());
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
        public async Task<bool> Validatefields()
        {
            IsSaveAttempted = true;
            ValidationMessage = null;

            if (string.IsNullOrWhiteSpace(SelectedAddress.ZipCode) ||
               !IsValidPinCode(SelectedAddress.ZipCode) ||
                string.IsNullOrWhiteSpace(SelectedAddress.Line1) ||
                string.IsNullOrWhiteSpace(SelectedAddress.Mobile1) ||
                string.IsNullOrWhiteSpace(SelectedAddress.Email) ||
                string.IsNullOrWhiteSpace(SelectedAddress.State) ||
                string.IsNullOrWhiteSpace(SelectedAddress.City) ||
                string.IsNullOrWhiteSpace(SelectedAddress.Locality) ||
                string.IsNullOrWhiteSpace(SelectedAddress.BranchUID) ||
                // string.IsNullOrWhiteSpace(SelectedAddress.ASMEmpUID) ||
                string.IsNullOrWhiteSpace(SelectedAddress.SalesOfficeUID) ||
                string.IsNullOrWhiteSpace(SelectedAddress.OrgUnitUID) ||
                string.IsNullOrWhiteSpace(SelectedAddress.ZipCode) ||
                string.IsNullOrWhiteSpace(SelfRegistrationUID ?? EditOnBoardingDetails.StoreAdditionalInfoCMI.SelfRegistrationUID))

            {
                ValidationMessage = "The following fields have invalid field(s)" + ": ";
                if (string.IsNullOrWhiteSpace(SelectedAddress.ZipCode) || !IsValidPinCode(SelectedAddress.ZipCode))
                {
                    ValidationMessage += "PinCode, ";
                }
                if (string.IsNullOrWhiteSpace(SelectedAddress.Line1))
                {
                    ValidationMessage += "Address1, ";
                }
                if (string.IsNullOrWhiteSpace(SelectedAddress.Mobile1))
                {
                    ValidationMessage += "Mobile Number, ";
                }

                if (string.IsNullOrWhiteSpace(SelectedAddress.Email))
                {
                    ValidationMessage += "Email, ";
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
                //if (string.IsNullOrWhiteSpace(SelectedAddress.ASMEmpUID))
                //{
                //    ValidationMessage += "Asm, ";
                //}~
                if (string.IsNullOrWhiteSpace(SelectedAddress.SalesOfficeUID))
                {
                    ValidationMessage += "Sales Office, ";
                }
                if (string.IsNullOrWhiteSpace(SelectedAddress.OrgUnitUID))
                {
                    ValidationMessage += "Organisation Unit, ";
                }
                if (string.IsNullOrWhiteSpace(SelfRegistrationUID ?? EditOnBoardingDetails.StoreAdditionalInfoCMI.SelfRegistrationUID))
                {
                    if (IsAsmMappedByCustomer)
                    {
                        if (!AsmDivisionDetails.Any())
                        {
                            ValidationMessage += "Division, ";
                            ValidationMessage += "Asm, ";
                        }
                        if (AsmDivisionDetails.Any(p => string.IsNullOrWhiteSpace(p.DivisionUID)))
                        {
                            ValidationMessage += "Division, ";
                        }
                        if (AsmDivisionDetails.Any(p => string.IsNullOrWhiteSpace(p.AsmEmpUID)))
                        {
                            ValidationMessage += "Asm, ";
                        }
                    }
                }
                if (string.IsNullOrWhiteSpace(SelfRegistrationUID ?? EditOnBoardingDetails.StoreAdditionalInfoCMI.SelfRegistrationUID)
                    && ValidationMessage == ("The following fields have invalid field(s)" + ": "))
                {
                    ValidationMessage = string.Empty;
                    return false;
                }
                ValidationMessage = ValidationMessage.TrimEnd(' ', ',');
                return true;
            }
            else
            {
                return false;
            }
        }
        public void AssignOU(IAddress address)
        {
            try
            {
                var selectedOUShipTo = OUselectionItemsForShipTo.Find(e => e.Code == address.OrgUnitUID);
                if (selectedOUShipTo != null)
                {
                    SelectedAddress.OrgUnitUID = selectedOUShipTo.UID;
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task SaveOrUpdate()
        {
            string actionType = OnboardingScreenConstant.Update;
            if (!IsBackBtnClicked)
            {
                //Validatefields();
            }

            try
            {
                if (!await Validatefields())
                {
                    if (!IsEditPage)
                    {
                        actionType = OnboardingScreenConstant.Create;
                        await OnAddAddress.InvokeAsync();
                        SelectedAddress.UID = Guid.NewGuid().ToString();
                        GenerateGridColumns();
                    }
                    else
                    {
                        await OnEditAddress.InvokeAsync();

                        if (IsEditPage)
                        {
                            IsShowAllShipToAddress = !IsShowAllShipToAddress;

                        }
                        GenerateGridColumns();
                    }
                    AsmDivisionDetails.ForEach(p =>
                    {
                        if (string.IsNullOrEmpty(p.LinkedItemUID))
                        {
                            p.LinkedItemType = "Address";
                            p.LinkedItemUID = SelectedAddress.UID;
                        }
                    });
                    AssignOU(SelectedAddress);
                    if (TabName == StoreConstants.Confirmed && !CustomerEditApprovalRequired)
                    {
                        await SaveOrUpdateAddress.InvokeAsync(SelectedAddress);
                        await RequestChange(actionType);
                    }
                    else if (TabName == StoreConstants.Confirmed)
                    {
                        await RequestChange(actionType);
                    }
                    else
                    {
                        await SaveOrUpdateAddress.InvokeAsync(SelectedAddress);
                    }

                    AsmDivisionDetailsDB = AsmDivisionDetails;
                    await SaveOrUpdateAsmDivision.InvokeAsync(AsmDivisionDetailsDB);
                    if (IsBillToAddressSuccess)
                    {
                        await OnClean();
                    }
                    Addresses.Clear();
                    Addresses.AddRange(await OnShowAllAddresssClick.Invoke());
                    await OnClean();
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
        //private void OnLocationChange(ILocationData locationMasterForUI)
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
            try
            {
                _loadingService.ShowLoading();
                await CheckboxChanged();
            }
            catch (Exception)
            {

            }
            finally
            {
                _loadingService.HideLoading();
                StateHasChanged();
            }
        }
        private async Task CheckboxChanged()
        {
            if (IsChecked)
            {
                IAddress? address = BillToAddresses.FirstOrDefault(e => e.IsDefault);
                if (address != null)
                {
                    SelectedAddress.Line1 = address.Line1;
                    SelectedAddress.Line2 = address.Line2;
                    SelectedAddress.Line3 = address.Line3;
                    SelectedAddress.ZipCode = address.ZipCode;
                    SelectedAddress.Mobile1 = address.Mobile1;
                    SelectedAddress.Mobile2 = address.Mobile2;
                    SelectedAddress.Email = address.Email;
                    SelectedAddress.State = address.State;
                    SelectedAddress.City = address.City;
                    SelectedAddress.Locality = address.Locality;
                    SelectedAddress.ZipCode = address.ZipCode;
                    SelectedAddress.BranchUID = address.BranchUID;
                    SelectedAddress.OrgUnitUID = address.OrgUnitUID;
                    await OnCopyInvokeAddressDropdowns.InvokeAsync(address);
                }

                else
                {
                    ShowNoPrimaryAddressPopup();
                    DeselectLocationDropDowns();
                    SelectedAddress.Line1 = string.Empty;
                    // SelectedAddress.Name = string.Empty; 
                    SelectedAddress.Line2 = string.Empty;
                    SelectedAddress.Line3 = string.Empty;
                    SelectedAddress.ZipCode = string.Empty;
                    SelectedAddress.Mobile1 = string.Empty;
                    SelectedAddress.Mobile2 = string.Empty;
                    SelectedAddress.Email = string.Empty;
                }
                StateHasChanged();
            }
            else
            {
                SelectedAddress = new Address();
                DeselectLocationDropDowns();
            }

        }
        private void ShowNoPrimaryAddressPopup()
        {
            isChecked = false;
            // Implement logic to show a popup or modal
            // For example, you can set a flag to control the visibility of a modal
            IsNoPrimaryAddressPopupVisible = true;
        }
        private bool IsNoPrimaryAddressPopupVisible { get; set; } = false;

        private void ClosePopup()
        {
            IsNoPrimaryAddressPopupVisible = false;
            StateHasChanged();
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
                await OUSelected.InvokeAsync(selecetedValue.UID);
            }
            else
            {
                SelectedAddress.OrgUnitUID = string.Empty;
            }
            StateHasChanged();
        }


        public async Task DivisionSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            //SelectedAddress.Line4 = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                obj.DivisionUID = selecetedValue?.UID;
                obj.DivisionName = selecetedValue?.Label;
                StateHasChanged();
            }
            else
            {
                obj.DivisionUID = string.Empty;
                obj.DivisionName = string.Empty;
            }
        }
        public async Task ASMSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            //SelectedAddress.Line4 = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                obj.AsmEmpUID = selecetedValue?.UID;
                obj.AsmEmpName = selecetedValue?.Label;
                StateHasChanged();
            }
            else
            {
                obj.AsmEmpUID = string.Empty;
                obj.AsmEmpName = string.Empty;
            }
        }
        public async Task SalesOfficeSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            //SelectedAddress.Line4 = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                SelectedAddress.SalesOfficeUID = selecetedValue?.UID;
                StateHasChanged();
            }
            else
            {
                SelectedAddress.SalesOfficeUID = string.Empty;
            }
        }

        private async Task OpenAsmPopUp()
        {
            IsAsmAdd = !IsAsmAdd;
        }
        private async void AddAsmDivisionDetails()
        {
            if (string.IsNullOrEmpty(obj.DivisionUID) || string.IsNullOrEmpty(obj.AsmEmpUID))
                return;
            if (!AsmDivisionDetails.Any(p => p.DivisionUID.Equals(obj.DivisionUID) && p.AsmEmpUID.Equals(obj.AsmEmpUID)))
            {
                IAsmDivisionMapping asmDetails = new AsmDivisionMapping();
                asmDetails = obj.DeepCopy();
                AsmDivisionDetails.Add(asmDetails);
                objCopy = asmDetails;
                //await SaveOrUpdateAsmDivision.InvokeAsync(AsmDivisionDetails);
            }
            StateHasChanged();
        }
        #region Change RequestLogic

        public async Task RequestChange(string actionType = OnboardingScreenConstant.Update)
        {
            SelectedAddress.Type = OnboardingScreenConstant.Shipping;
            SelectedAddress.LinkedItemType = OnboardingScreenConstant.LinkedItemTypeStore;

            List<IChangeRecordDTO> ChangeRecordDTOs = new List<IChangeRecordDTO>
            {
                new ChangeRecordDTO
                {
                    LinkedItemUID=LinkedItemUID,
                    Action= actionType,
                    ScreenModelName = OnboardingScreenConstant.ShiptoAddress,
                    UID = SelectedAddress.UID,
                    ChangeRecords = CommonFunctions.GetChangedData(CommonFunctions.CompareObjects(OriginalSelectedAddress, SelectedAddress)!)
                }
            }
            .Where(c => c.ChangeRecords.Count > 0)
            .ToList();
            if (ChangeRecordDTOs.Count > 0)
            {
                var ChangeRecordDTOInJson = CommonFunctions.ConvertToJson(ChangeRecordDTOs);
                await InsertDataInChangeRequest.InvokeAsync(ChangeRecordDTOInJson);
                SelectedAddress = ResetAddress();
                OriginalSelectedAddress = ResetAddress();
            }
            ChangeRecordDTOs.Clear();
        }
        //public object GetModifiedObject(IStoreAdditionalInfoCMI storeAdditionalinfoCMI)
        //{
        //    var modifiedObject = new
        //    {
        //        storeAdditionalinfoCMI.NoOfManager,
        //        storeAdditionalinfoCMI.NoOfSalesTeam,
        //        storeAdditionalinfoCMI.NoOfCommercial,
        //        storeAdditionalinfoCMI.NoOfService,
        //        storeAdditionalinfoCMI.TotalEmp,
        //        storeAdditionalinfoCMI.NoOfOthers
        //    };

        //    return modifiedObject;
        //}
        #endregion
    }
}
