using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System.Security.Claims;
using Winit.Modules.Address.Model.Interfaces;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Constants;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.BroadClassification.Model.Constant;
using Winit.Modules.Contact.Model.Interfaces;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.FileSys.Model.Classes;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Constants;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.UIModels.Common;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Common.GST;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using WinIt.Pages.ApprovalRoleEngine;


namespace WinIt.Pages.Customer_Details
{
    public partial class AddCustomerDetails
    {

        //Contact
        private List<IContact> allContacts = new List<IContact>();
        public ContactDetails _contactDetails { get; set; }

        [CascadingParameter]
        public EventCallback<WinIt.BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public bool IsLoad { get; set; }
        public bool IsAddFetchPopUp { get; set; }
        public string gstNumber { get; set; }
        public List<FilterModel> ColumnsForFilter;
        public string Billing = "Billing";
        public string Shipping = "Shipping";
        public bool IsSection { get; set; }
        public bool? IsEwwCmi { get; set; }
        public bool IsAsm { get; set; } = false;
        public bool HideMSME { get; set; } = true;
        public bool IsMultipleASM { get; set; } = false;
        public bool HideVendor { get; set; } = true;
        public bool IsAsmMappedByCustomer { get; set; }
        public bool IsContactRendered { get; set; } = false;
        public bool IsBilltoAddressRendered { get; set; } = false;
        public bool IsShiptoAddressRendered { get; set; } = false;
        public bool IsKartaRendered { get; set; } = false;
        public bool IsAsmMappingRendered { get; set; } = false;
        public bool IsServiceCenterRendered { get; set; } = false;
        public bool IsEmployeeDetailsRendered { get; set; } = false;
        public bool IsShowRoomDetailsRendered { get; set; } = false;
        public bool IsBankersDetailsRendered { get; set; } = false;
        public bool IsBusinessDetailsRendered { get; set; } = false;
        public bool IsDistBusinessDetailsRendered { get; set; } = false;
        public bool IsEarlierWorkedCMIRendered { get; set; } = false;
        public bool IsAreaofDistAgreedRendered { get; set; } = false;
        public bool IsAreaofOperationAgreedRendered { get; set; } = false;
        public bool IsDocumentAppendixRendered { get; set; } = false;
        public bool IsCodeofEthicsRendered { get; set; } = false;
        public CustomerInformation _customerInformation { get; set; }
        public BillToAddress _billtoAddressDetails { get; set; }
        public ShipToAddress _shiptoAddressDetails { get; set; }
        public ServiceCenterDetails _serviceCentreDetails { get; set; }
        public EmployeeDetails _employeeDetails { get; set; }
        public ShowRoomDetails _showroomDetails { get; set; }
        public BankersDetails _bankersDetails { get; set; }
        public BusinessDetails _businessDetails { get; set; }
        public DistributorBusinessDetails _distBusinessDetails { get; set; }
        public EarlierWorkedWithCMI _earlierWorkedwithCMIDetails { get; set; }
        public AreaofDistributionAgreed _areaofDistributonAgreedDetails { get; set; }
        public AreaofOperationAgreed _areaofOperationAgreedDetails { get; set; }
        public DocumentsAppendixA _documentAppendixADetails { get; set; }
        public CodeEthicsConductPartners _codeEthicsConductPartners { get; set; }
        public Dictionary<string, List<EmployeeDetail>>? ApprovalUserCodes { get; set; }
        public string OrgUID { get; set; }
        public string contactclass { get; set; }
        private bool IsCardVisible = true;
        public bool IsEdit { get; set; }
        public bool IsEditOnBoardDetails { get; set; }

        public bool IsBillToAddressSuccess = false;
        public bool IsAsmMappingSuccess { get; set; }
        public bool IsSuccess { get; set; }
        public string CustomerID { get; set; }
        public string ValidationMessage;

        public string SelfRegistrationUID { get; set; }
        //  public string TabName { get; set; } = "";
        public bool IsSuccessCustomerInformation { get; set; }
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "OnBoard New Partner",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="OnBoard New Partner",URL="CustomerDetails",IsClickable=true},
                new BreadCrumModel(){SlNo=1,Text="OnBoard New Partner"},
            }
        };
        public List<IAllApprovalRequest> AllApprovalLevelList { get; set; } = new List<IAllApprovalRequest>();
        public List<ISelectionItem> CustomerClassificationselectionItems { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> ClassificationTypeselectionItems { get; set; } = new List<ISelectionItem>();
        public bool IsInitialised { get; set; }
        public List<ISelectionItem> AsmselectionItems { get; set; } = new List<ISelectionItem>();
        public List<IAsmDivisionMapping> AsmDivisionMappingDetails { get; set; } = new List<IAsmDivisionMapping>();
        public List<ApprovalStatusResponse> ApprovalTracker { get; set; } = new List<ApprovalStatusResponse>();
        private bool isFirstTimeSave { get; set; } = true;
        public string BranchUID { get; set; } = "";
        public IEmp BranchBM { get; set; } = new Winit.Modules.Emp.Model.Classes.Emp();
        public bool IsCorrectBM { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            ClaimsPrincipal user = authState.User;
            if (user.Identities.Any(p => p.IsAuthenticated))
            {
                _customerDetailsViewModel.IsLogin = false;
                await base.OnInitializedAsync();
            }
            else
            {
                _customerDetailsViewModel.IsLogin = true;
            }

            IsLoad = true;
            toggle.IsCustomerDetails = true;
            Store = _serviceProvider.CreateInstance<IStore>();
        }
        public int RuleId { get; set; }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!string.IsNullOrWhiteSpace(ValidationMessage))
            {
                await Task.Delay(5000); // Wait for 5 seconds
                ValidationMessage = string.Empty; // Clear the validation message
                StateHasChanged(); // Refresh the component
            }
        }

        protected override async void OnInitialized()
        {
            try
            {
                _loadingService.ShowLoading();
                IsEditOnBoardDetails = Convert.ToBoolean(GetParameterValueFromURL("IsEditOnBoardDetails"));
                _customerDetailsViewModel.TabName = GetParameterValueFromURL("Tab");
                Console.WriteLine(_customerDetailsViewModel.TabName);
                CustomerID = GetParameterValueFromURL("UID");
                SelfRegistrationUID = GetParameterValueFromURL("SUID");
                await OnBoardingSetup();
                if (IsEditOnBoardDetails)
                {
                    await _customerDetailsViewModel.GetGstDetails(gstNumber);
                    AllApprovalLevelList = await _customerDetailsViewModel.GetAllApproveListDetails(CustomerID ?? SelfRegistrationUID);

                    if (AllApprovalLevelList != null && AllApprovalLevelList.Count > 0)
                    {
                        RequestId = int.Parse(AllApprovalLevelList[0]?.RequestID);
                        var approvalUserDetail = AllApprovalLevelList[0]?.ApprovalUserDetail;
                        ApprovalUserCodes = string.IsNullOrEmpty(approvalUserDetail)
                        ? new Dictionary<string, List<EmployeeDetail>>()
                        : DeserializeApprovalUserCodes(approvalUserDetail) ?? new Dictionary<string, List<EmployeeDetail>>();




                    }
                    if (_customerDetailsViewModel.TabName == StoreConstants.Confirmed)
                    {
                        _customerDetailsViewModel._originalOnBoardCustomerDTO = _serviceProvider.CreateInstance<IOnBoardCustomerDTO>();
                        _customerDetailsViewModel._originalOnBoardCustomerDTO.Store = (_customerDetailsViewModel?._onBoardCustomerDTO.Store as Winit.Modules.Store.Model.Classes.Store).DeepCopy()!;
                        _customerDetailsViewModel._originalOnBoardCustomerDTO.StoreAdditionalInfo = (_customerDetailsViewModel._onBoardCustomerDTO.StoreAdditionalInfo as StoreAdditionalInfo).DeepCopy()!;
                        _customerDetailsViewModel._originalOnBoardCustomerDTO.StoreAdditionalInfoCMI = (_customerDetailsViewModel._onBoardCustomerDTO.StoreAdditionalInfoCMI as StoreAdditionalInfoCMI).DeepCopy()!;
                        _customerDetailsViewModel._originalOnBoardCustomerDTO.FileSys = new List<Winit.Modules.FileSys.Model.Interfaces.IFileSys>();
                        List<FileSys> concreteFileSysList = new List<FileSys>();
                        foreach (var fileSys in _customerDetailsViewModel._onBoardCustomerDTO.FileSys)
                        {
                            if (fileSys is FileSys concreteFileSys)
                            {
                                concreteFileSysList.Add(concreteFileSys); // Add to concrete list
                            }
                        }
                        var serializedFileSys = JsonConvert.SerializeObject(concreteFileSysList);
                        var deepCopiedFileSys = JsonConvert.DeserializeObject<List<FileSys>>(serializedFileSys);
                        _customerDetailsViewModel._originalOnBoardCustomerDTO.FileSys = deepCopiedFileSys?.ConvertAll(item => (Winit.Modules.FileSys.Model.Interfaces.IFileSys)item);
                    }
                }
                _customerDetailsViewModel.SelfRegistrationUID = SelfRegistrationUID ?? _customerDetailsViewModel.EditOnBoardingDetails.StoreAdditionalInfoCMI.SelfRegistrationUID;

                base.OnInitialized();
                IsInitialised = true;
                _loadingService.HideLoading();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                _loadingService.HideLoading();
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
        public async Task OnBoardingSetup()
        {
            try
            {
                if (string.IsNullOrEmpty(CustomerID))
                {
                    Winit.Modules.Store.Model.Classes.Store store = await _customerDetailsViewModel.CheckStoreExistsOrNot(SelfRegistrationUID);
                    if (store != null)
                    {
                        IsEditOnBoardDetails = true;
                    }
                    else
                    {
                        _customerDetailsViewModel.SelfRegistrationUID = SelfRegistrationUID;
                    }
                }
                _customerDetailsViewModel.StoreUID = CustomerID ?? SelfRegistrationUID;
                _customerDetailsViewModel.GenerateCustomerCode();
                await _customerDetailsViewModel.CreateInstances();
                await _customerDetailsViewModel.PopulateOnBoardDetails(CustomerID ?? SelfRegistrationUID);
                if (IsEditOnBoardDetails)
                {
                    FirmType = _customerDetailsViewModel.EditOnBoardingDetails.StoreAdditionalInfo.FirmType;
                    await _customerDetailsViewModel.MapOnBoardDetails();
                    BranchUID = _customerDetailsViewModel.EditOnBoardingDetails.Address.Where(p => p.Type == OnboardingScreenConstant.Billing && p.IsDefault).FirstOrDefault().BranchUID;
                    BranchBM = await _customerDetailsViewModel.GetBMByBranchUID(BranchUID);
                    await CheckForBM();
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {

            }
        }
        public void IsMultipleASMAllowed(bool IsMultipleAsmAllow)
        {
            IsMultipleASM = IsMultipleAsmAllow;
            StateHasChanged();
        }
        public void HideSection(bool MSME)
        {
            HideMSME = MSME;
            StateHasChanged();
        }
        public void HideVendorSection(bool Vendor)
        {
            HideVendor = Vendor;
            StateHasChanged();
        }
        public void GetIsAsmMapped(bool isAsmMapped)
        {
            IsAsmMappedByCustomer = isAsmMapped;
            StateHasChanged();
        }
        private string GetParameterValueFromURL(string paramName)
        {
            var uri = new Uri(NavigationManager.Uri);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);

            return queryParams.Get(paramName);
            //return "b5c6d66b-5c41-4d9b-9b0d-ddacc7697b89";
        }
        public string FirmType { get; set; }
        public string AooaType { get; set; }

        protected async Task GetBroadClassification(string broadClassification)
        {
            _customerDetailsViewModel.BroadClassfication = broadClassification;
        }
        protected async Task GetFirmType(string firmType)
        {
            FirmType = firmType;
        }
        protected async Task GetAooaType(string aooaType)
        {
            AooaType = aooaType;
        }
        public async Task OnChangeBroadClassification(string UID)
        {
            try
            {
                await _customerDetailsViewModel.OnChangeBroadClassification(UID);
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnStateSelection(List<string> UID)
        {
            try
            {
                await _customerDetailsViewModel.OnStateSelection(UID);
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnCitySelection(List<string> UID)
        {
            try
            {
                await _customerDetailsViewModel.OnCitySelection(UID);
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnLocalitySelection(List<string> UID)
        {
            try
            {
                await _customerDetailsViewModel.OnLocalitySelection(UID);
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnBranchSelection(string UID)
        {
            try
            {
                await _customerDetailsViewModel.OnBranchSelection(UID);
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnStateSelectionForShipTo(List<string> UID)
        {
            try
            {
                await _customerDetailsViewModel.OnStateSelectionForShipTo(UID);
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnCitySelectionForShipTo(List<string> UID)
        {
            try
            {
                await _customerDetailsViewModel.OnCitySelectionForShipTo(UID);
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnLocalitySelectionForShipTo(List<string> UID)
        {
            try
            {
                await _customerDetailsViewModel.OnLocalitySelectionForShipTo(UID);
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnBranchSelectionForShipTo(string UID)
        {
            try
            {
                await _customerDetailsViewModel.OnBranchSelectionForShipTo(UID);
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnOUSelectionForShipTo(string ParentUID)
        {
            try
            {
                await _customerDetailsViewModel.OnOUSelectionForShipTo(ParentUID);
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        //public async Task OnEditSelectionForBillTo(string UID)
        //{
        //    try
        //    {
        //        await _customerDetailsViewModel.OnEditSelectionForBillTo(UID);
        //        StateHasChanged();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        public async Task OnEditSelectionBillTo(IAddress address)
        {
            try
            {
                await _customerDetailsViewModel.OnEditSelectionForBillTo(address);
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnCopyGSTBillTo(GSTINDetailsModel gSTINDetails)
        {
            try
            {
                await _customerDetailsViewModel.OnCopyGSTBillTo(gSTINDetails);
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnEditSelectionForShipTo(IAddress address)
        {
            try
            {
                await _customerDetailsViewModel.OnEditSelectionForShipTo(address);
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnEditSelectionForShowRoom(IStoreShowroom storeShowroom)
        {
            try
            {
                await _customerDetailsViewModel.OnEditSelectionForShowRoom(storeShowroom);
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnCopySelectionForShipTo(IAddress address)
        {
            try
            {
                await _customerDetailsViewModel.OnCopySelectionForShipTo(address);
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task InvokeAsmDivision(string UID)
        {
            try
            {
                await _customerDetailsViewModel.GetAsmDivisionDetails(UID);
                _shiptoAddressDetails.AsmDivisionDetails = _customerDetailsViewModel.AsmDivisionMappingDetails;
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task DeleteAsmDivision(IAsmDivisionMapping asmDivisionMapping)
        {
            try
            {
                await _customerDetailsViewModel.DeleteAsmDivisionMapping(asmDivisionMapping.UID);
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnStateSelectionForShowRoom(List<string> UID)
        {
            try
            {
                await _customerDetailsViewModel.OnStateSelectionForShowRoom(UID);
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnCitySelectionForShowRoom(List<string> UID)
        {
            try
            {
                await _customerDetailsViewModel.OnCitySelectionForShowRoom(UID);
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnLocalitySelectionForShowRoom(List<string> UID)
        {
            try
            {
                await _customerDetailsViewModel.OnLocalitySelectionForShowRoom(UID);
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task AddFetchDetails()
        {
            try
            {
                _loadingService.ShowLoading();
                if (string.IsNullOrEmpty(gstNumber))
                {
                    _tost.Add("GST", "Please enter GST Number", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                    return;
                }
                else
                {
                    if (await _customerDetailsViewModel.IsGstUnique(gstNumber))
                    {
                        bool IsSuccess = await _customerDetailsViewModel.GetGstDetails(gstNumber);
                        if (IsSuccess)
                        {

                            IsAddFetchPopUp = true;
                            IsSection = true;
                            IsCardVisible = false;
                        }
                        else
                        {
                            _tost.Add("GST", "Please enter valid GST number", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                        }
                    }
                    else
                    {
                        _tost.Add("GST", "GST number already exists", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                    }

                }

            }
            catch (Exception ex)
            {
                _tost.Add("GST", "GST Server is down. Please try again later.", Winit.UIComponents.SnackBar.Enum.Severity.Error);
            }
            finally
            {
                _loadingService.HideLoading();
            }

        }
        public async Task SaveGstDetails()
        {
            IsAddFetchPopUp = false;
            IsSection = true;
            IsCardVisible = false;

            //Call GST third party API
        }
        public async Task OnBack()
        {
            IsAddFetchPopUp = false;
        }
        public string SectionRender { get; set; }
        private void ToggleSectionDetails(string SectionName)
        {
            SectionRender = SectionName;
            try
            {
                if (SectionName == OnboardingScreenConstant.Contact)
                {
                    toggle.IsContactDetails = !toggle.IsContactDetails;
                    IsContactRendered = true;
                }
                else if (SectionName == OnboardingScreenConstant.BilltoAddress)
                {
                    toggle.IsBillToAddress = !toggle.IsBillToAddress;
                    IsBilltoAddressRendered = true;
                }
                else if (SectionName == OnboardingScreenConstant.ShiptoAddress)
                {
                    toggle.IsShipToAddress = !toggle.IsShipToAddress;
                    IsShiptoAddressRendered = true;
                }
                else if (SectionName == OnboardingScreenConstant.Karta)
                {
                    toggle.IsKarta = !toggle.IsKarta;
                    IsKartaRendered = true;
                }
                else if (SectionName == OnboardingScreenConstant.AsmMapping)
                {
                    toggle.IsAsmMapping = !toggle.IsAsmMapping;
                    IsAsmMappingRendered = true;
                }
                else if (SectionName == OnboardingScreenConstant.ServiceCenterDetail)
                {
                    toggle.IsServiceCenterDetails = !toggle.IsServiceCenterDetails;
                    IsServiceCenterRendered = true;
                }
                else if (SectionName == OnboardingScreenConstant.EmployeeDetails)
                {
                    toggle.IsEmployeeDetails = !toggle.IsEmployeeDetails;
                    IsEmployeeDetailsRendered = true;
                }
                else if (SectionName == OnboardingScreenConstant.ShowroomDetails)
                {
                    toggle.IsShowRoomDetails = !toggle.IsShowRoomDetails;
                    IsShowRoomDetailsRendered = true;
                }
                else if (SectionName == OnboardingScreenConstant.BankersDetails)
                {
                    toggle.IsBankersDetails = !toggle.IsBankersDetails;
                    IsBankersDetailsRendered = true;
                }
                else if (SectionName == OnboardingScreenConstant.BusinessDetails)
                {
                    toggle.IsBusinessDetails = !toggle.IsBusinessDetails;
                    IsBusinessDetailsRendered = true;
                }
                else if (SectionName == OnboardingScreenConstant.DistBusinessDetails)
                {
                    toggle.IsDistributorBusinessDetails = !toggle.IsDistributorBusinessDetails;
                    IsDistBusinessDetailsRendered = true;
                }
                else if (SectionName == OnboardingScreenConstant.EarlierWorkWithCMI)
                {
                    toggle.IsEarlierWorkedWithCMI = !toggle.IsEarlierWorkedWithCMI;
                    IsEarlierWorkedCMIRendered = true;
                }
                else if (SectionName == OnboardingScreenConstant.AreaOfDistAgreed)
                {
                    toggle.IsAreaofDistributionAgreed = !toggle.IsAreaofDistributionAgreed;
                    IsAreaofDistAgreedRendered = true;
                }
                else if (SectionName == OnboardingScreenConstant.AreaofOperationAgreed)
                {
                    toggle.IsAreaofOperationAgreed = !toggle.IsAreaofOperationAgreed;
                    IsAreaofOperationAgreedRendered = true;
                }
                else if (SectionName == OnboardingScreenConstant.DocumentAppendix)
                {
                    toggle.IsDocumentsAppendixA = !toggle.IsDocumentsAppendixA;
                    IsDocumentAppendixRendered = true;
                }
                else if (SectionName == OnboardingScreenConstant.CodeofEthics)
                {
                    toggle.IsCodeEthicsConductPartners = !toggle.IsCodeEthicsConductPartners;
                    IsCodeofEthicsRendered = true;
                }
            }
            catch (Exception ex)
            {

            }

        }
        public bool IsBackBtnClicked { get; set; } = false;
        private async Task BackBtnClicked()
        {
            //IsBackBtnClicked = true;
            //bool isContact = false;
            //if (_contactDetails != null)
            //{
            //    isContact = _contactDetails.AreContactsEqual();
            //}
            //// bool isBilltoAddress = _billtoAddressDetails.AreBillToAddressEqual();
            //bool userConfirmed = await _AlertMessgae.ShowConfirmationReturnType("Alert", "Are you sure you want to go  back? No Issues,All your data saved as draft", "Yes", "No");
            //if (userConfirmed && isContact) // Replace with your logic
            //{
            //    await _contactDetails.SaveOrUpdate();
            //}
            ////if (userConfirmed && isBilltoAddress)
            ////{
            ////    await _billtoAddressDetails.SaveOrUpdate();
            ////}
            //if (userConfirmed && (_shiptoAddressDetails != null ? _shiptoAddressDetails.IsSuccess : false))
            //{
            //    await _shiptoAddressDetails.SaveOrUpdate();
            //}
            //else if (userConfirmed && SectionRender == OnboardingScreenConstant.ServiceCenterDetail)
            //{
            //    // Call the API to store data
            //    await _serviceCentreDetails.CreateUpdateServiceCenterDetails();
            //    // await _serviceCentreDetails.SaveFileSys();
            //    //await SaveOrUpdateeEmployeeDetails(_serviceCentreDetails.StoreAdditionalInfoCmi);
            //    //await CreateUpdateDocumentAppendix1(_serviceCentreDetails.DocumentAppendixfileSysList);
            //    if (_serviceCentreDetails.ValidationMessage == null)
            //    {
            //        _navigationManager.NavigateTo("CustomerDetails");
            //    }
            //    return;
            //}
            //else if (userConfirmed && SectionRender == OnboardingScreenConstant.EmployeeDetails)
            //{
            //    await _employeeDetails.SaveOrUpdate();
            //    if (_employeeDetails.ValidationMessage == null)
            //    {
            //        _navigationManager.NavigateTo("CustomerDetails");
            //    }
            //    return;
            //}
            //else if (userConfirmed && SectionRender == OnboardingScreenConstant.ShowroomDetails)
            //{

            //    _showroomDetails.AddShowroomDetails();
            //    //  await SaveOrUpdateeEmployeeDetails(_showroomDetails.storeAdditionalInfoCMI);
            //    if (_showroomDetails.ValidationMessage == null)
            //    {
            //        await _showroomDetails.SaveUpdateDeleteShowroomDetails();
            //        _navigationManager.NavigateTo("CustomerDetails");
            //    }
            //    return;
            //}
            //else if (userConfirmed && SectionRender == OnboardingScreenConstant.BankersDetails)
            //{
            //    _bankersDetails.AddBankingDetails();
            //    if (_bankersDetails.ValidationMessage == null)
            //    {
            //        await _bankersDetails.SaveUpdateDeleteBankers();
            //        _navigationManager.NavigateTo("CustomerDetails");
            //    }
            //    return;
            //}
            //else if (userConfirmed && SectionRender == OnboardingScreenConstant.BusinessDetails)
            //{
            //    await _businessDetails.SaveBusinessDetails();
            //    if (_businessDetails.ValidationMessage == null)
            //    {
            //        _navigationManager.NavigateTo("CustomerDetails");
            //    }
            //    return;
            //}
            //else if (userConfirmed && SectionRender == OnboardingScreenConstant.DistBusinessDetails)
            //{
            //    await _distBusinessDetails.SaveUpdateDistBusiness();
            //    if (_distBusinessDetails.ValidationMessage == null)
            //    {
            //        _navigationManager.NavigateTo("CustomerDetails");
            //    }
            //    return;
            //}
            //else if (userConfirmed && SectionRender == OnboardingScreenConstant.EarlierWorkWithCMI)
            //{
            //    // await SaveOrUpdateeEmployeeDetails(_earlierWorkedwithCMIDetails.StoreAdditionalInfoCMI);
            //    await _earlierWorkedwithCMIDetails.SaveOrUpdate();
            //    if (_earlierWorkedwithCMIDetails.ValidationMessage == null)
            //    {
            //        _navigationManager.NavigateTo("CustomerDetails");
            //    }
            //    return;
            //}
            //else if (userConfirmed && SectionRender == OnboardingScreenConstant.AreaOfDistAgreed)
            //{
            //    // await SaveOrUpdateeEmployeeDetails(_earlierWorkedwithCMIDetails.StoreAdditionalInfoCMI);
            //    await _areaofDistributonAgreedDetails.SaveOrUpdate();
            //    if (_areaofDistributonAgreedDetails.ValidationMessage == null && _areaofDistributonAgreedDetails._onBoardCustomerDTO.StoreAdditionalInfoCMI.IsAgreedWithTNC)
            //    {
            //        _navigationManager.NavigateTo("CustomerDetails");
            //    }
            //    return;
            //}
            //else if (userConfirmed && SectionRender == OnboardingScreenConstant.AreaofOperationAgreed)
            //{
            //    await _areaofOperationAgreedDetails.SaveOrUpdate();
            //    if (_areaofOperationAgreedDetails.ValidationMessage == null)
            //    {
            //        _navigationManager.NavigateTo("CustomerDetails");
            //    }
            //    return;
            //}
            //else if (userConfirmed && SectionRender == OnboardingScreenConstant.DocumentAppendix)
            //{
            //    await _documentAppendixADetails.SaveFileSys();
            //    if (_documentAppendixADetails.IsSuccess)
            //    {
            //        _navigationManager.NavigateTo("CustomerDetails");
            //    }
            //    return;
            //}
            //else if (userConfirmed && SectionRender == OnboardingScreenConstant.CodeofEthics)
            //{
            //    await _codeEthicsConductPartners.GetsavedImagePath(_codeEthicsConductPartners.fileSysList);
            //    if (_codeEthicsConductPartners.IsSuccess)
            //    {
            //        _navigationManager.NavigateTo("CustomerDetails");
            //    }
            //    return;
            //}
            //else
            //{
            try
            {
                bool userConfirmed = await _AlertMessgae.ShowConfirmationReturnType("Alert", "Are you sure you want to go  back?", "Yes", "No");
                if (userConfirmed)
                {
                    if (IsSelfRegistration())
                    {
                        _navigationManager.NavigateTo("/");
                        return;
                    }
                    _navigationManager.NavigateTo("CustomerDetails");
                }
            }
            catch (Exception)
            {

                throw;
            }
            //}
        }
        public List<IAddress> Addresses { get; set; } = new List<IAddress>();
        public async Task InsertGstAddress()
        {
            try
            {
                foreach (var gstAddress in _customerDetailsViewModel.ShipToAddressFromGst)
                {
                    IAddress address = new Winit.Modules.Address.Model.Classes.Address()
                    {
                        Line1 = gstAddress.Address,
                        State = gstAddress.AR_ADR_State,
                        IsDefault = gstAddress.IsPrimary,
                        Type = "Shipping"
                    };
                    Addresses.Add(address);
                }
                if (_customerDetailsViewModel.BillToAddressFromGst != null)
                {
                    IAddress address2 = new Winit.Modules.Address.Model.Classes.Address()
                    {
                        Line1 = _customerDetailsViewModel.BillToAddressFromGst.PR_ADR_DoorNo + ", " + _customerDetailsViewModel.BillToAddressFromGst.PR_ADR_FloorNo + ", " +
                                                        _customerDetailsViewModel.BillToAddressFromGst.AR_ADR_BuildingName,
                        Line2 = _customerDetailsViewModel.BillToAddressFromGst.PR_ADR_Street + ", " + _customerDetailsViewModel.BillToAddressFromGst.PR_ADR_Location,
                        State = _customerDetailsViewModel.BillToAddressFromGst.PR_ADR_State,
                        City = _customerDetailsViewModel.BillToAddressFromGst.PR_ADR_District,
                        IsDefault = _customerDetailsViewModel.BillToAddressFromGst.IsPrimary,
                        Type = "Billing"
                    };
                    Addresses.Add(address2);
                }
                await _customerDetailsViewModel.PopulateGstAddress(Addresses);
            }
            catch (Exception ex)
            {

            }
        }
        protected async Task SaveOrUpdateCustomerInformation(IOnBoardCustomerDTO? onBoardCustomerDTO)
        {
            _loadingService.ShowLoading();
            if (!IsEditOnBoardDetails && string.IsNullOrEmpty(_customerDetailsViewModel.CustomerCreationStoreUID))
            {
                if (await _customerDetailsViewModel.SaveUpdateCustomerInformation(onBoardCustomerDTO, true))
                {
                    if (isFirstTimeSave)
                    {
                        await InsertGstAddress();
                        isFirstTimeSave = false;
                    }
                    _tost.Add("Customer", "Customer details Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                    IsSuccessCustomerInformation = true;
                    _customerDetailsViewModel.CustomerCreationStoreUID = onBoardCustomerDTO.Store.UID;
                }
                else
                {
                    ShowErrorSnackBar("Error", "Failed to save...");
                }
            }
            else
            {
                if (await _customerDetailsViewModel.SaveUpdateCustomerInformation(onBoardCustomerDTO, false))
                {
                    _tost.Add("Customer", "Customer details Updated Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                }
                else
                {
                    ShowErrorSnackBar("Error", "Failed to Update...");
                }
            }
            _loadingService.HideLoading();
            //await AsmMappingOnLoad();
        }
        protected async Task OnAddContact(IContact contact)
        {
            IsEdit = false;
        }
        protected async Task OnEditContact(IContact contact)
        {
            IsEdit = true;
        }
        protected async Task OnAddBill_ShipToAddress(IAddress contact)
        {
            IsEdit = false;
        }
        protected async Task OnEditBill_ShipToAddress(IAddress contact)
        {
            IsEdit = true;
        }
        protected async Task OnAddShowroom(IStoreShowroom storeShowroom)
        {
            IsEdit = false;
        }
        protected async Task OnEditShowroom(IStoreShowroom storeShowroom)
        {
            IsEdit = true;
        }
        private bool IsContactDetailsContactUnique(IContact? contact)
        {
            if (contact == null)
            {
                return false;
            }

            // Check if any contact already exists with the same mobile1, mobile2, or email
            return !_customerDetailsViewModel.ContactList.Any(c =>
                 c.Id != contact.Id && // Exclude the current contact if editing
        (
            (c.Phone == contact.Phone && !string.IsNullOrEmpty(contact.Phone)) ||
            (c.PhoneExtension == contact.PhoneExtension && !string.IsNullOrEmpty(contact.PhoneExtension)) ||
            (c.Email == contact.Email && !string.IsNullOrEmpty(contact.Email))
        )
    );
        }
        private bool IsBill_ShiptoAddressContactUnique(IAddress? address)
        {
            if (address.Type == Billing)
            {
                if (address == null)
                {
                    return false;
                }

                // Check if any contact already exists with the same mobile1, mobile2, or email
                return !_customerDetailsViewModel.AddressList.Where(s => s.Type == Billing).Any(c =>
                     c.Id != address.Id && // Exclude the current contact if editing
            (

                (c.Mobile1 == address.Mobile1 && !string.IsNullOrEmpty(address.Mobile1)) ||
                (c.Mobile2 == address.Mobile2 && !string.IsNullOrEmpty(address.Mobile2)) ||
                (c.Email == address.Email && !string.IsNullOrEmpty(address.Email))
            )
        );
            }
            else
            {
                if (address == null)
                {
                    return false;
                }

                // Check if any contact already exists with the same mobile1, mobile2, or email
                return !_customerDetailsViewModel.AddressList.Where(s => s.Type == Shipping).Any(c =>
                     c.Id != address.Id && // Exclude the current contact if editing
            (
                (c.Mobile1 == address.Mobile1 && !string.IsNullOrEmpty(address.Mobile1)) ||
                (c.Mobile2 == address.Mobile2 && !string.IsNullOrEmpty(address.Mobile2)) ||
                (c.Email == address.Email && !string.IsNullOrEmpty(address.Email))
            )
        );
            }
        }
        public bool IsContactSuccess { get; set; } = false;
        protected async Task SaveOrUpdateContact(IContact? contact)
        {
            _loadingService.ShowLoading();
            if (!IsContactDetailsContactUnique(contact))
            {
                ShowErrorSnackBar("Error", "A contact with the same mobile number or email already exists.");
                _loadingService.HideLoading();
                return;
            }
            if (contact.IsDefault)
            {
                // Fetch the list of existing contacts (adjust according to your data source)
                var existingContacts = await _customerDetailsViewModel.GetAllContacts();

                // Find the currently primary contact
                var primaryContact = existingContacts.FirstOrDefault(c => c.IsDefault && c.Id != contact.Id);

                if (primaryContact != null)
                {
                    // Show an error if a primary contact already exists
                    ShowErrorSnackBar("Error", "Kindly check there is already one primary contact saved if you want to make it as primary then make the existing contact as not primary.");
                    _loadingService.HideLoading();
                    return;
                }
            }
            if (!IsEdit)
            {
                if (_customerDetailsViewModel.StoreUID != null)
                {
                    if (await _customerDetailsViewModel.SaveUpdateContact(contact, true))
                    {
                        if (IsBackBtnClicked)
                        {
                            _tost.Add("", "Data Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);

                        }
                        else
                        {
                            _tost.Add("Contact", "Contact details Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                            IsContactSuccess = true;
                            await _contactDetails.OnClean();
                        }

                    }
                    else
                    {
                        ShowErrorSnackBar("Error", "Failed to save...");
                    }
                }
                else
                {
                    ShowErrorSnackBar("Error", "First fill the Customer Information...");
                }
            }
            else
            {
                if (await _customerDetailsViewModel.SaveUpdateContact(contact, false))
                {
                    _tost.Add("Contact", "Contact details Updated Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                    IsContactSuccess = true;
                    await _contactDetails.OnClean();
                }
                else
                {
                    ShowErrorSnackBar("Error", "Failed to Update...");
                }
            }
            _loadingService.HideLoading();
        }
        protected async Task SaveOrUpdateBillToAddress(IAddress? address)
        {
            IsBillToAddressSuccess = false;
            _loadingService.ShowLoading();
            address.Type = Billing;
            //if (!IsBill_ShiptoAddressContactUnique(address))
            //{
            //    ShowErrorSnackBar("Error", "A address with the same mobile number or email already exists.");
            //    _loadingService.HideLoading();
            //    return;
            //}
            if (address.IsDefault)
            {
                // Fetch the list of existing contacts (adjust according to your data source)
                var existingContacts = await _customerDetailsViewModel.GetAllAddress(Billing);

                // Find the currently primary contact
                var primaryContact = existingContacts.FirstOrDefault(c => c.IsDefault && c.UID != address.UID);

                if (primaryContact != null)
                {
                    // Show an error if a primary contact already exists
                    ShowErrorSnackBar("Error", "Kindly check there is already one primary address saved if you want to make it as primary then make the existing address as not primary.");
                    _loadingService.HideLoading();
                    return;
                }
            }
            if (!IsEdit)
            {
                if (_customerDetailsViewModel.StoreUID != null)
                {
                    if (await _customerDetailsViewModel.SaveUpdateBillToAddress(address, true))
                    {
                        if (IsBackBtnClicked)
                        {
                            _tost.Add("", "Data Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                        }
                        else
                        {
                            _tost.Add("Bill To Address", "Bill To Address details Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                            IsBillToAddressSuccess = true;
                        }
                    }
                    else
                    {
                        ShowErrorSnackBar("Error", "Failed to save...");
                    }
                }
                else
                {
                    ShowErrorSnackBar("Error", "First fill the Customer Information...");
                }
            }
            else
            {
                if (await _customerDetailsViewModel.SaveUpdateBillToAddress(address, false))
                {
                    if (IsBackBtnClicked)
                    {
                        _tost.Add("", "Data Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                    }
                    else
                    {
                        _tost.Add("Bill To Address", "Bill To Address details Updated Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                        IsBillToAddressSuccess = true;
                    }
                }
                else
                {
                    ShowErrorSnackBar("Error", "Failed to Update...");
                }
            }
            _loadingService.HideLoading();
        }
        protected async Task AsmMappingOnLoad()
        {
            try
            {
                if (toggle.IsAsmMapping || toggle.IsShipToAddress)
                {
                    await _customerDetailsViewModel.AsmOnload();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {

            }
        }
        protected async Task SaveOrUpdateAsmDivision(List<IAsmDivisionMapping> asmDivisionMapping)
        {
            if (!IsEdit)
            {
                IsAsmMappingSuccess = await _customerDetailsViewModel.SaveUpdateAsmDivision(asmDivisionMapping, true);
            }
            else
            {
                IsAsmMappingSuccess = await _customerDetailsViewModel.SaveUpdateAsmDivision(asmDivisionMapping, false);
            }
            StateHasChanged();
        }
        protected async Task SaveOrUpdateShipToAddress(IAddress? address)
        {
            _loadingService.ShowLoading();
            if (!IsBill_ShiptoAddressContactUnique(address))
            {
                ShowErrorSnackBar("Error", "A contact with the same mobile number or email already exists.");
                _loadingService.HideLoading();
                return;
            }
            if (address.IsDefault)
            {
                // Fetch the list of existing contacts (adjust according to your data source)
                var existingContacts = await _customerDetailsViewModel.GetAllAddress(Shipping);

                // Find the currently primary contact
                var primaryContact = existingContacts.FirstOrDefault(c => c.IsDefault && c.Id != address.Id);

                if (primaryContact != null)
                {
                    // Show an error if a primary contact already exists
                    ShowErrorSnackBar("Error", "Kindly check there is already one primary address saved if you want to make it as primary then make the existing address as not primary.");
                    _loadingService.HideLoading();
                    return;
                }
            }
            if (!IsEdit)
            {
                if (_customerDetailsViewModel.StoreUID != null)
                {
                    if (await _customerDetailsViewModel.SaveUpdateShipToAddress(address, true))
                    {
                        if (IsBackBtnClicked)
                        {
                            _tost.Add("", "Data Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                        }
                        else
                        {
                            _tost.Add("Ship To Address", "Ship To Address details Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                        }
                    }
                    else
                    {
                        ShowErrorSnackBar("Error", "Failed to save...");
                    }
                }
                else
                {
                    ShowErrorSnackBar("Error", "First fill the Customer Information...");
                }
            }
            else
            {
                if (await _customerDetailsViewModel.SaveUpdateShipToAddress(address, false))
                    if (IsBackBtnClicked)
                    {
                        _tost.Add("", "Data Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                    }
                    else
                    {
                        _tost.Add("Ship To Address", "Ship To Address Details Updated Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                    }
                else
                {
                    ShowErrorSnackBar("Error", "Failed to Update...");
                }
            }
            _loadingService.HideLoading();
        }
        public List<NameAndAddressOfProprietorModel> KartaDetails()
        {
            if (_customerDetailsViewModel.KartaEditDetails.Count > 0)
            {
                return _customerDetailsViewModel.KartaEditDetails;
            }
            else
            {
                return _customerDetailsViewModel.KartaEditDetails = new List<NameAndAddressOfProprietorModel> { new NameAndAddressOfProprietorModel { Sn = 1 } };
            }
        }
        protected async Task SaveOrUpdateeEmployeeDetails(IStoreAdditionalInfoCMI? storeAdditionalInfoCMI)
        {
            storeAdditionalInfoCMI.UID = _customerDetailsViewModel.StoreAdditionalInfoCMIUid;
            _loadingService.ShowLoading();
            if (_customerDetailsViewModel.StoreUID != null)
            {
                if (await _customerDetailsViewModel.SaveUpdateEmployeeDetails(storeAdditionalInfoCMI))
                {
                    if (storeAdditionalInfoCMI.Action == "Delete")
                    {
                        _tost.Add($"{storeAdditionalInfoCMI.SectionName}", $"{storeAdditionalInfoCMI.SectionName} details Delete Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                    }
                    else
                    {
                        if (!IsEditOnBoardDetails)
                        {
                            _tost.Add($"{storeAdditionalInfoCMI.SectionName}", $"{storeAdditionalInfoCMI.SectionName} details Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                        }
                        else
                        {
                            _tost.Add($"{storeAdditionalInfoCMI.SectionName}", $"{storeAdditionalInfoCMI.SectionName} details Updated Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                        }
                    }
                }
                else
                {
                    if (storeAdditionalInfoCMI.Action == "Delete")
                    {
                        ShowErrorSnackBar("Error", "Failed to Delete...");
                    }
                    else
                    {
                        ShowErrorSnackBar("Error", "Failed to save...");
                    }
                }
            }
            else
            {
                ShowErrorSnackBar("Error", "First fill the Customer Information...");
            }
            _loadingService.HideLoading();
        }
        public async Task InsertPdfInFileSys(List<IFileSys> fileSysList)
        {
            try
            {
                if (_customerDetailsViewModel.StoreUID != null)
                {
                    if (fileSysList.Count > 0)
                    {
                        ShowLoader();
                        _ = await _customerDetailsViewModel.CreateUpdateDocumentFileSysData(fileSysList, true);
                        _ = _tost.Add("File", "File Uploaded Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                    }
                    else
                    {
                        _ = _tost.Add("File", "File Uploaded Failed", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                    }
                }
                else
                {
                    ShowErrorSnackBar("Error", "First fill the Customer Information...");
                }
                HideLoader();
            }
            catch (Exception ex)
            {

            }
        }
        public void MakeCodeOfEthics()
        {
            _customerDetailsViewModel.IsCodeOfEthics = false;
            StateHasChanged();
        }

        protected async Task OnEditContactGridview(IContact contact)
        {
            _loadingService.ShowLoading();
            _customerDetailsViewModel.Contact = contact;
            _loadingService.HideLoading();
        }
        protected async Task OnEditBill_ShipAddressGridview(IAddress address)
        {
            _loadingService.ShowLoading();
            _customerDetailsViewModel.Address = address;
            _loadingService.HideLoading();
        }
        protected async Task OnDeleteContact(string contactUID)
        {

            if (await _AlertMessgae.ShowConfirmationReturnType("Delete", "Are you sure want to Delete this?"))
            {
                try
                {
                    string msg = await _customerDetailsViewModel.DeleteContactDetails(contactUID);
                    if (msg.Contains("Failed"))
                    {
                        await _AlertMessgae.ShowErrorAlert("Failed", msg);
                    }
                    else
                    {
                        await _AlertMessgae.ShowSuccessAlert("Success", msg);
                    }

                }
                catch (Exception ex)
                {

                    await _AlertMessgae.ShowSuccessAlert("deleted", ex.Message);
                }
            }

            StateHasChanged();
        }
        protected async Task OnDeleteBill_ShiptoAddress(string addressUID)
        {
            IsBillToAddressSuccess = false;
            if (await _AlertMessgae.ShowConfirmationReturnType("Delete", "Are you sure want to Delete this?"))
            {
                try
                {
                    string msg = await _customerDetailsViewModel.DeleteAddressDetails(addressUID);
                    if (msg.Contains("Failed"))
                    {
                        await _AlertMessgae.ShowErrorAlert("Failed", msg);
                        IsBillToAddressSuccess = true;
                    }
                    else
                    {
                        _tost.Add("Ship To Address", "Bill To Address Deleted Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);

                    }

                }
                catch (Exception ex)
                {

                    await _AlertMessgae.ShowSuccessAlert("deleted", ex.Message);
                }
            }

            StateHasChanged();
        }
        public string LinkeditemUID { get; set; }
        public async Task CreateUpdateDocument(List<IFileSys> fileSys)
        {
            if (fileSys.Count > 0)
            {
                ShowLoader();
                _ = await _customerDetailsViewModel.CreateUpdateDocumentFileSysData(fileSys, true);
                StateHasChanged();
                _ = _tost.Add("File", "File Uploaded Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
            }
            else
            {
                _ = _tost.Add("File", "File Uploaded Failed", Winit.UIComponents.SnackBar.Enum.Severity.Error);
            }
            HideLoader();
        }
        private List<List<IFileSys>> _fileSysList;

        public async Task CreateUpdateDocumentAppendix(List<List<IFileSys>> fileSys)
        {
            _fileSysList = fileSys;
            if (_customerDetailsViewModel.StoreUID != null)
            {
                if (fileSys.Count > 0)
                {
                    ShowLoader();
                    bool result = await _customerDetailsViewModel.CreateUpdateDocumentFileSysDataAppendix(fileSys);
                    if (result)
                    {
                        _ = _tost.Add("File", "Files Uploaded Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                    }
                    else
                    {
                        _ = _tost.Add("File", "File Upload Failed", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                    }
                    StateHasChanged();
                }
                else
                {
                    _ = _tost.Add("File", "Pllease Upload Files", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                }
            }
            else
            {
                ShowErrorSnackBar("Error", "First fill the Customer Information...");
            }
            HideLoader();
        }
        public async Task CreateUpdateDocumentAppendix1(List<List<IFileSys>> fileSys)
        {
            if (_customerDetailsViewModel.StoreUID != null)
            {
                if (fileSys.Count > 0)
                {
                    ShowLoader();
                    bool result = await _customerDetailsViewModel.CreateUpdateDocumentFileSysDataAppendix(fileSys);
                    StateHasChanged();
                }
                else
                {
                    //_ = _tost.Add("File", "Pllease Upload Files", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                }
            }
            else
            {
                ShowErrorSnackBar("Error", "First fill the Customer Information...");
            }
            HideLoader();
        }
        public async Task AsmConfirm()
        {
            Store.UID = _customerDetailsViewModel.StoreUID;
            Store.Status = SalesOrderStatus.PENDING;
            StoreApprovalDTO storeApprovalDTO = new StoreApprovalDTO();
            storeApprovalDTO.Store = Store;
            _loadingService.ShowLoading();
            if (await _customerDetailsViewModel.SaveUpdateDistDetails(storeApprovalDTO))
            {
                _tost.Add("Data", "Data Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                // await executeRule();
                _navigationManager.NavigateTo("CustomerDetails");
            }
            else
            {
                ShowErrorSnackBar("Error", "Failed to save...");
                await JSRuntime.InvokeVoidAsync("focustotable", "top");

            }
            _loadingService.HideLoading();
        }
        public async Task ConfirmedDetails()
        {
            try
            {
                StoreApprovalDTO storeApprovalDTO = new StoreApprovalDTO();
                storeApprovalDTO.Store = Store;
                await JSRuntime.InvokeVoidAsync("focustotable", "top");
                if (await CheckValidations())
                {
                    Store.UID = _customerDetailsViewModel.StoreUID;
                    Store.Status = IsSelfRegistration() ? SalesOrderStatus.PENDING_FROM_BM : SalesOrderStatus.PENDING;
                    _loadingService.ShowLoading();
                    if (await _customerDetailsViewModel.SaveUpdateDistDetails(storeApprovalDTO))
                    {
                        _tost.Add("Data", "Data Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                        if (IsSelfRegistration())
                        {
                            //await executeRule();
                            _navigationManager.NavigateTo("/");
                            _loadingService.HideLoading();
                            return;
                        }
                        _navigationManager.NavigateTo("CustomerDetails");
                    }
                    else
                    {
                        ShowErrorSnackBar("Error", "Failed to save...");
                        await JSRuntime.InvokeVoidAsync("focustotable", "top");

                    }
                }
                else
                {
                    await JSRuntime.InvokeVoidAsync("focustotable", "top");
                    // Handle the case where validation fails
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _loadingService.HideLoading();
            }

        }

        public async Task<bool> CheckValidations()
        {
            try
            {
                if (await GetMandatoryfields())
                {
                    return true;
                }
                else if (sectionsWithErrors.Count == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool IsSelfRegistration()
        {
            try
            {
                if (string.IsNullOrEmpty(SelfRegistrationUID) && string.IsNullOrEmpty(_customerDetailsViewModel.storeAdditionalInfoCMI.SelfRegistrationUID))
                    return false;
                else
                    return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task CheckForBM()
        {
            try
            {
                if (BranchBM.LoginId == _iAppUser.Emp.LoginId && _customerDetailsViewModel.EditOnBoardingDetails.Store.Status == SalesOrderStatus.PENDING_FROM_BM)
                {
                    if (_customerDetailsViewModel.BroadClassfication == StoreConstants.Trader || _customerDetailsViewModel.BroadClassfication == StoreConstants.Service)
                    {
                        AsmselectionItems = await _customerDetailsViewModel.GetAllAsemByBranchUID(BranchUID);
                    }
                    else
                    {
                        AsmselectionItems = await _customerDetailsViewModel.GetAllAsmByBranchUID(BranchUID);
                    }
                    IsCorrectBM = true;
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
        public bool CheckReportingEmpUID()
        {
            try
            {
                if (_iAppUser != null)
                {
                    return _iAppUser.Emp.UID == _customerDetailsViewModel.EditOnBoardingDetails.Store.ReportingEmpUID;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public string UserCode { get; set; } = "423";//Usercode From Session
                                                     //public string RoleCode { get; set; } = "ASEM";//RoleCode From Session
        private bool Loading { get; set; } = false;
        private bool Error { get; set; } = false;
        private string? ErrorMessage { get; set; }
        public int RequestId { get; set; }
        private ApprovalEngine? childComponentRef;


        private async Task<ApprovalActionResponse> HandleApprovalAction(ApprovalStatusUpdate approvalStatusUpdate)
        {
            StoreApprovalDTO storeApprovalDTO = new StoreApprovalDTO();
            storeApprovalDTO.Store = Store;
            ApprovalActionResponse approvalActionResponse = new ApprovalActionResponse();
            try
            {
                if (!approvalStatusUpdate.IsFinalApproval && approvalStatusUpdate.Status != ApprovalConst.Rejected)
                {
                    approvalActionResponse.IsApprovalActionRequired = false;
                    return approvalActionResponse;
                }
                approvalActionResponse.IsApprovalActionRequired = true;
                storeApprovalDTO.ApprovalStatusUpdate = approvalStatusUpdate;
                if (approvalStatusUpdate.Status == ApprovalConst.Rejected)
                {
                    storeApprovalDTO.Store.Status = ApprovalConst.Rejected;
                    approvalActionResponse.IsSuccess = await _customerDetailsViewModel.SaveUpdateDistDetails(storeApprovalDTO);
                    return approvalActionResponse;
                }
                storeApprovalDTO.Store.Status = ApprovalConst.Approved;
                Store.UID = _customerDetailsViewModel.StoreUID;
                Store.Status = SalesOrderStatus.APPROVED;
                //_loadingService.ShowLoading();
                if (await _customerDetailsViewModel.SaveUpdateDistDetails(storeApprovalDTO))
                {
                    _customerDetailsViewModel._onBoardCustomerDTO.Store.FranchiseeOrgUID = _iAppUser.SelectedJobPosition.OrgUID ?? "";
                    bool isSuccess = await _customerDetailsViewModel.CreateDistributorAfterFinalApproval(_customerDetailsViewModel._onBoardCustomerDTO.Store);
                    if (isSuccess)
                    {
                        _tost.Add("Data", "Customer OnBoarded Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                        //_loadingService.HideLoading();
                    }
                    //_tost.Add("Data", "Data Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                    // await executeRule();
                    _navigationManager.NavigateTo("CustomerDetails");
                    StateHasChanged();
                }
                else
                {
                    ShowErrorSnackBar("Error", "Failed to save store status...");
                }
                approvalActionResponse.IsSuccess = await _customerDetailsViewModel.SaveUpdateDistDetails(storeApprovalDTO);
                return approvalActionResponse;
            }
            catch (CustomException ex)
            {

                approvalActionResponse.IsSuccess = false;
                return approvalActionResponse;
            }
        }

        //private async Task HandleApproved()
        //{
        //    try
        //    {
        //        StoreApprovalDTO storeApprovalDTO = new StoreApprovalDTO();
        //        storeApprovalDTO.Store = Store;
        //        //change store status
        //        Store.UID = _customerDetailsViewModel.StoreUID;
        //        Store.Status = SalesOrderStatus.APPROVED;
        //        //_loadingService.ShowLoading();
        //        if (await _customerDetailsViewModel.SaveUpdateDistDetails(storeApprovalDTO))
        //        {
        //            _customerDetailsViewModel._onBoardCustomerDTO.Store.FranchiseeOrgUID = _iAppUser.SelectedJobPosition.OrgUID ?? "";
        //            bool isSuccess = await _customerDetailsViewModel.CreateDistributorAfterFinalApproval(_customerDetailsViewModel._onBoardCustomerDTO.Store);
        //            if (isSuccess)
        //            {
        //                _tost.Add("Data", "Customer OnBoarded Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
        //                //_loadingService.HideLoading();
        //            }
        //            //_tost.Add("Data", "Data Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
        //            // await executeRule();
        //            _navigationManager.NavigateTo("CustomerDetails");
        //            StateHasChanged();
        //        }
        //        else
        //        {
        //            ShowErrorSnackBar("Error", "Failed to save store status...");
        //        }
        //        //if (await _customerDetailsViewModel.CreatePushedDataStatus())
        //        //{
        //        //    //_tost.Add("Data", "Data Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);

        //        //}
        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //}
        //private async Task HandleError(string error)
        //{
        //    // Set the error message
        //    ErrorMessage = error;
        //    await _alertService.ShowErrorAlert("Approval Engine", ErrorMessage);
        //}
        //private async Task HandleUpdateRequest(ApprovalApiResponse<dynamic> res)
        //{
        //    ErrorMessage = res.Message;
        //    if (res.Success)
        //    {
        //        await _alertService.ShowSuccessAlert("Success", ErrorMessage);

        //    }
        //    else
        //    {
        //        await _alertService.ShowErrorAlert("Error", ErrorMessage);
        //    }
        //}


        //private async Task HandleExecuteRule(ApprovalApiResponse<ApprovalStatus> res)
        //{
        //    if (res.Success)
        //    {
        //        RequestId = (int)res.data.RequestId;
        //        //insert into AllApprovalRequest(linkedItemType,linkedItemUID,requestID)values('Store',StoreUID,RequestId.Tostring())
        //    }
        //    else
        //        await _alertService.ShowErrorAlert("Error", res.Message);
        //}
        //public async Task CreateAllApprovalRequestDetails(long requestId)
        //{
        //    await _customerDetailsViewModel.GetAllUserHierarchy(UserHierarchyTypeConst.Emp, ReportingUID(), RuleId);
        //    await _customerDetailsViewModel.SaveAllApprovalRequestDetails(requestId.ToString());
        //}
        //public string ReportingUID()
        //{

        //    if (string.IsNullOrEmpty(SelfRegistrationUID ?? _customerDetailsViewModel.EditOnBoardingDetails.StoreAdditionalInfoCMI.SelfRegistrationUID))
        //    {
        //        return _iAppUser.Emp.UID;
        //    }
        //    return _customerDetailsViewModel.EditOnBoardingDetails.Store.ReportingEmpUID;
        //}
        public async Task OnApprovalTracker(List<ApprovalStatusResponse> approvalStatusResponses)
        {
            ApprovalTracker = approvalStatusResponses;
        }
        public bool CheckApprovalTracker()
        {
            try
            {
                if (ApprovalTracker != null && ApprovalTracker.Count > 0)
                {
                    int highestApprovedLevel = 0;
                    if (ApprovalTracker.Any(x => x.Status == "Approved"))
                    {
                        highestApprovedLevel = ApprovalTracker.Where(x => x.Status == "Approved")
                                                              .Max(x => x.ApproverLevel);
                    }
                    foreach (var item in ApprovalTracker)
                    {
                        if (_iAppUser?.Role?.Code == item.ApproverId && item.ApproverLevel == highestApprovedLevel + 1)
                        {
                            return true;
                        }
                    }
                    return false;
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
        //private async Task OnReject()
        //{
        //    //change store status
        //    Store.UID = _customerDetailsViewModel.StoreUID;
        //    Store.Status = SalesOrderStatus.REJECTED;
        //    //_loadingService.ShowLoading();
        //    if (await _customerDetailsViewModel.SaveUpdateDistDetails(Store))
        //    {
        //        //_customerDetailsViewModel._onBoardCustomerDTO.Store.FranchiseeOrgUID = _iAppUser.SelectedJobPosition.OrgUID ?? "";
        //        //bool isSuccess = ((CustomerDetailsWebViewModel)_customerDetailsViewModel).CreateDistributorAfterFinalApproval(_customerDetailsViewModel._onBoardCustomerDTO.Store).Result;
        //        //if (isSuccess)
        //        //{

        //        //    //_loadingService.HideLoading();
        //        //}
        //        _tost.Add("Reject", "Successfully rejected", Winit.UIComponents.SnackBar.Enum.Severity.Success);
        //        _navigationManager.NavigateTo("CustomerDetails");

        //    }
        //    else
        //    {
        //        ShowErrorSnackBar("Error", "Failed to save store status...");
        //    }


        //}

        //#######################
        public IStore Store { get; set; }
        protected async Task SaveOrUpdateDistDetails()
        {
            try
            {
                Store.UID = _customerDetailsViewModel.StoreUID;
                Store.Status = SalesOrderStatus.DRAFT;
                StoreApprovalDTO storeApprovalDTO = new StoreApprovalDTO();
                storeApprovalDTO.Store = Store;
                _loadingService.ShowLoading();
                if (await _customerDetailsViewModel.SaveUpdateDistDetails(storeApprovalDTO))
                {
                    if (IsSelfRegistration())
                    {
                        _tost.Add("Data", "Data Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                        _navigationManager.NavigateTo("/");
                        return;
                    }
                    _tost.Add("Data", "Data Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                    _navigationManager.NavigateTo("CustomerDetails");
                }
                else
                {
                    ShowErrorSnackBar("Error", "Failed to save...");
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _loadingService.HideLoading();
                StateHasChanged();
            }
        }
        public async Task<bool> GetMandatoryfields()
        {
            try
            {
                // Initialize validation results
                bool isCustomerValid = await ValidateCustomerDetails();
                bool isContactValid = await ValidateContactDetails();
                bool isBillToAddressValid = await ValidateBilltoAddressDetails();
                bool isShipToAddressValid = await ValidateShiptoAddressDetails();
                bool isAsmMappingValid = await ValidateAsmMappingDetails();
                bool isServiceCenterValid = true;
                bool isAreaofOperationAgreedValid = true;
                bool isAreaofDistAgreedValid = true;
                if (_customerDetailsViewModel.BroadClassfication == BroadClassificationConst.SERVICE)
                {
                    isServiceCenterValid = await ValidateServiceCenterDetails();
                    isAreaofOperationAgreedValid = await ValidateAreaofOperationAgreed();
                }

                bool isEmployeeValid = true;
                bool isShowroomValid = true;
                bool isBusinessDetailsValid = true;
                bool isDistBusinessDetailsValid = true;
                if (_customerDetailsViewModel.BroadClassfication != BroadClassificationConst.SERVICE && _customerDetailsViewModel.BroadClassfication != BroadClassificationConst.CPAUSERVICEPARTNER)
                {
                    isEmployeeValid = await ValidateEmployeeDetails();
                    isShowroomValid = await ValidateShowroomDetails();
                }
                if (_customerDetailsViewModel.BroadClassfication == BroadClassificationConst.CPAUSERVICEPARTNER)
                {
                    isEmployeeValid = await ValidateEmployeeDetails();
                }
                if (_customerDetailsViewModel.BroadClassfication == BroadClassificationConst.DIST)
                {
                    isDistBusinessDetailsValid = await ValidateDistBusinessDetails();
                    isBusinessDetailsValid = await ValidateBusinessDetails();
                    isAreaofDistAgreedValid = await ValidateAreaofDistAgreed();
                }

                bool isBankersDetailsValid = await ValidateBankersDetails();
                //bool isEarlierWorkCMiValid = await ValidateEarlierWorkedCMIDetails();
                bool isDocumentAppendixValid = await ValidateDocumentAppendix();
                bool isCodeofEthicsValid = await ValidateCodeofEthics();
                bool isTermsAndConditionsValid = await ValidateTermsAndConditions();

                // Combine results
                bool allValid = isCustomerValid && isContactValid && isBillToAddressValid && isShipToAddressValid && isAsmMappingValid &&
                                (_customerDetailsViewModel.BroadClassfication == BroadClassificationConst.SERVICE ? isServiceCenterValid : true) &&
                                (_customerDetailsViewModel.BroadClassfication != BroadClassificationConst.SERVICE ? (isEmployeeValid && isShowroomValid && isBusinessDetailsValid && isDistBusinessDetailsValid) : true) &&
                                isBankersDetailsValid && isAreaofDistAgreedValid && isDocumentAppendixValid && isCodeofEthicsValid && isTermsAndConditionsValid;

                // Handle the final validation message
                await FinalizeValidation();

                return allValid;
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                ValidationMessage = $"An error occurred: {ex.Message}";
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private async Task<bool> FinalizeValidation()
        {
            // Clear any previously added error messages based on the current context
            if (_customerDetailsViewModel.BroadClassfication == BroadClassificationConst.SERVICE)
            {
                sectionsWithErrors.Remove("Employee Details");
                sectionsWithErrors.Remove("Showroom Details");
                sectionsWithErrors.Remove("Business Details");
            }
            else
            {
                sectionsWithErrors.Remove("Service Centre Details");
                sectionsWithErrors.Remove("Area of Operation Agreed");

            }
            if (_customerDetailsViewModel.BroadClassfication == BroadClassificationConst.CPAUSERVICEPARTNER)
            {
                sectionsWithErrors.Remove("Showroom Details");
            }
            if (_customerDetailsViewModel.BroadClassfication != BroadClassificationConst.DIST)
            {
                sectionsWithErrors.Remove("Area of Distribution Agreed");
                sectionsWithErrors.Remove("Business Details(Distributor)");
                sectionsWithErrors.Remove("Business Details");
            }
            if (sectionsWithErrors.Any())
            {
                ValidationMessage = $"Please fill the mandatory sections: {string.Join(", ", sectionsWithErrors)}";
                return false;
            }
            else
            {
                ValidationMessage = string.Empty;
                Console.WriteLine("Validation successful");
                return true;
            }
        }

        private HashSet<string> sectionsWithErrors = new HashSet<string>
        {
            "Customer Information",
            "Contact Details",
            "Bill To Address Details",
            "Ship To Address Details",
            "Asm Mapping",
            "Service Centre Details",
            "Employee Details",
            "Showroom Details",
            "Bankers Details",
            "Business Details",
            "Business Details(Distributor)",
            //"Earlier Worked with CMI",
            "Area of Distribution Agreed",
            "Area of Operation Agreed",
            "Document Appendix",
            "Code of Ethics",
            "Terms And Conditions"
        };
        // Initialize a dictionary to track sections with errors
        public async Task IsEwwCMISelected(bool? IsEwwCmi)
        {
            try
            {
                this.IsEwwCmi = IsEwwCmi;
                StateHasChanged();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<bool> ValidateCustomerDetails()
        {
            try
            {
                bool hasErrors =
                    string.IsNullOrEmpty(_customerDetailsViewModel._onBoardCustomerDTO?.Store?.BroadClassification) ||
                    string.IsNullOrEmpty(_customerDetailsViewModel._onBoardCustomerDTO.StoreAdditionalInfo?.FirmType);

                if (hasErrors)
                {
                    sectionsWithErrors.Add("Customer Information");
                }
                else
                {
                    sectionsWithErrors.Remove("Customer Information");
                }

                return await FinalizeValidation();
            }
            catch (Exception ex)
            {
                ValidationMessage = $"An error occurred: {ex.Message}";
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> ValidateContactDetails()
        {
            try
            {
                // Check if the Contacts list is null or empty
                bool listIsEmpty = _customerDetailsViewModel.Contacts == null || !_customerDetailsViewModel.Contacts.Any();

                if (listIsEmpty)
                {
                    // If the list is null or empty, add it to the error sections
                    sectionsWithErrors.Add("Contact Details");
                }
                else
                {
                    // Remove from error sections if the list has elements
                    sectionsWithErrors.Remove("Contact Details");
                }

                return await FinalizeValidation();
            }
            catch (Exception ex)
            {
                ValidationMessage = $"An error occurred: {ex.Message}";
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> ValidateBilltoAddressDetails()
        {
            try
            {
                bool hasErrors = _customerDetailsViewModel.BillToAddresses == null || !_customerDetailsViewModel.BillToAddresses.Any() || !_customerDetailsViewModel.BillToAddresses.Any(p => p.IsDefault);

                if (hasErrors)
                {
                    sectionsWithErrors.Add("Bill To Address Details");
                    if (!_customerDetailsViewModel.ShipToAddresses.Any(p => p.IsDefault))
                    {
                        _tost.Add("Alert", "Atleast One Primary address is needed in BillTo Address", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                    }
                }
                else
                {
                    sectionsWithErrors.Remove("Bill To Address Details");
                }

                return await FinalizeValidation();
            }
            catch (Exception ex)
            {
                ValidationMessage = $"An error occurred: {ex.Message}";
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> ValidateShiptoAddressDetails()
        {
            try
            {
                bool hasErrors = _customerDetailsViewModel.ShipToAddresses == null || !_customerDetailsViewModel.ShipToAddresses.Any() || !_customerDetailsViewModel.ShipToAddresses.Any(p => p.IsDefault);

                if (hasErrors)
                {
                    sectionsWithErrors.Add("Ship To Address Details");
                    if (!_customerDetailsViewModel.ShipToAddresses.Any(p => p.IsDefault))
                    {
                        _tost.Add("Alert", "Atleast One Primary address is needed in ShipTo Address", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                    }
                }
                else
                {
                    sectionsWithErrors.Remove("Ship To Address Details");
                }

                return await FinalizeValidation();
            }
            catch (Exception ex)
            {
                ValidationMessage = $"An error occurred: {ex.Message}";
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public async Task<bool> ValidateAsmMappingDetails()
        {
            try
            {
                if (!IsSelfRegistration() && IsAsmMappedByCustomer)// no self registration and isasmmapped be true
                {

                    bool hasErrors = _customerDetailsViewModel.AsmMappingGridDetails == null || !_customerDetailsViewModel.AsmMappingGridDetails.Any();

                    if (hasErrors)
                    {
                        sectionsWithErrors.Add("Asm Mapping");
                    }
                    else
                    {
                        sectionsWithErrors.Remove("Asm Mapping");
                    }
                }
                else
                {
                    sectionsWithErrors.Remove("Asm Mapping");
                }
                return await FinalizeValidation();
            }
            catch (Exception ex)
            {
                ValidationMessage = $"An error occurred: {ex.Message}";
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public async Task<bool> ValidateServiceCenterDetails()
        {
            try
            {
                bool hasErrors =
                    string.IsNullOrEmpty(_customerDetailsViewModel.EditOnBoardingDetails.StoreAdditionalInfoCMI.ScAddress) ||
                    string.IsNullOrEmpty(_customerDetailsViewModel.EditOnBoardingDetails.StoreAdditionalInfoCMI.ScCurrentBrandHandled);

                if (hasErrors)
                {
                    sectionsWithErrors.Add("Service Centre Details");
                }
                else
                {
                    sectionsWithErrors.Remove("Service Centre Details");
                }

                return await FinalizeValidation();
            }
            catch (Exception ex)
            {
                ValidationMessage = $"An error occurred: {ex.Message}";
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public async Task<bool> ValidateEmployeeDetails()
        {
            try
            {
                bool hasErrors =
                    string.IsNullOrEmpty(_customerDetailsViewModel.EditOnBoardingDetails.StoreAdditionalInfoCMI.NoOfManager.ToString()) ||
                    string.IsNullOrEmpty(_customerDetailsViewModel.EditOnBoardingDetails.StoreAdditionalInfoCMI.NoOfSalesTeam.ToString()) ||
                    string.IsNullOrEmpty(_customerDetailsViewModel.EditOnBoardingDetails.StoreAdditionalInfoCMI.NoOfCommercial.ToString()) ||
                    string.IsNullOrEmpty(_customerDetailsViewModel.EditOnBoardingDetails.StoreAdditionalInfoCMI.NoOfService.ToString()) ||
                    string.IsNullOrEmpty(_customerDetailsViewModel.EditOnBoardingDetails.StoreAdditionalInfoCMI.NoOfOthers.ToString());

                if (hasErrors)
                {
                    sectionsWithErrors.Add("Employee Details");
                }
                else
                {
                    sectionsWithErrors.Remove("Employee Details");
                }

                return await FinalizeValidation();
            }
            catch (Exception ex)
            {
                ValidationMessage = $"An error occurred: {ex.Message}";
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public async Task<bool> ValidateShowroomDetails()
        {
            try
            {
                bool hasErrors = _customerDetailsViewModel.ShowroomDetails == null || !_customerDetailsViewModel.ShowroomDetails.Any();

                if (hasErrors)
                {
                    sectionsWithErrors.Add("Showroom Details");
                }
                else
                {
                    sectionsWithErrors.Remove("Showroom Details");
                }

                return await FinalizeValidation();
            }
            catch (Exception ex)
            {
                ValidationMessage = $"An error occurred: {ex.Message}";
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public async Task<bool> ValidateBankersDetails()
        {
            try
            {
                bool hasErrors = _customerDetailsViewModel.SignatoryDetails == null || !_customerDetailsViewModel.SignatoryDetails.Any();

                if (hasErrors)
                {
                    sectionsWithErrors.Add("Bankers Details");
                }
                else
                {
                    sectionsWithErrors.Remove("Bankers Details");
                }

                return await FinalizeValidation();
            }
            catch (Exception ex)
            {
                ValidationMessage = $"An error occurred: {ex.Message}";
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public async Task<bool> ValidateBusinessDetails()
        {
            try
            {
                bool hasErrors =
                    string.IsNullOrEmpty(_customerDetailsViewModel.storeAdditionalInfoCMI.ProductDealingIn) ||
                    string.IsNullOrEmpty(_customerDetailsViewModel.storeAdditionalInfoCMI.AreaOfOperation) ||
                    string.IsNullOrEmpty(_customerDetailsViewModel.storeAdditionalInfoCMI.DistProducts) ||
                    string.IsNullOrEmpty(_customerDetailsViewModel.storeAdditionalInfoCMI.DistAreaOfOperation);

                if (hasErrors)
                {
                    sectionsWithErrors.Add("Business Details");
                }
                else
                {
                    sectionsWithErrors.Remove("Business Details");
                }

                return await FinalizeValidation();
            }
            catch (Exception ex)
            {
                ValidationMessage = $"An error occurred: {ex.Message}";
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public async Task<bool> ValidateDistBusinessDetails()
        {
            try
            {

                //bool hasErrors = _customerDetailsViewModel.RetailingCityMonthlySalesList == null || !_customerDetailsViewModel.RetailingCityMonthlySalesList.Any() && _customerDetailsViewModel.RACSalesByYearList == null || !_customerDetailsViewModel.RACSalesByYearList.Any();
                bool hasErrors = string.IsNullOrEmpty(_customerDetailsViewModel.StoreAdditionalInfoCMI.DistNoOfSubDealers.ToString());


                if (hasErrors)
                {
                    sectionsWithErrors.Add("Business Details(Distributor)");
                }
                else
                {
                    sectionsWithErrors.Remove("Business Details(Distributor)");
                }

                return await FinalizeValidation();
            }
            catch (Exception ex)
            {
                ValidationMessage = $"An error occurred: {ex.Message}";
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public async Task<bool> ValidateEarlierWorkedCMIDetails()
        {
            try
            {
                if (IsEwwCmi == true)
                {
                    bool hasErrors =
                        string.IsNullOrEmpty(_earlierWorkedwithCMIDetails.StoreAdditionalInfoCMI.EwwYearOfOperationAndVolume) ||
                        string.IsNullOrEmpty(_earlierWorkedwithCMIDetails.StoreAdditionalInfoCMI.EwwDealerInfo) ||
                        string.IsNullOrEmpty(_earlierWorkedwithCMIDetails.StoreAdditionalInfoCMI.EwwNameOfFirms);

                    if (hasErrors)
                    {
                        sectionsWithErrors.Add("Earlier Worked with CMI");
                    }
                    else
                    {
                        sectionsWithErrors.Remove("Earlier Worked with CMI");
                    }

                    return await FinalizeValidation();
                }
                else
                {
                    sectionsWithErrors.Remove("Earlier Worked with CMI");

                    return true;
                }
            }
            catch (Exception ex)
            {
                ValidationMessage = $"An error occurred: {ex.Message}";
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public async Task<bool> ValidateAreaofDistAgreed()
        {
            try
            {
                bool hasErrors =
                    string.IsNullOrEmpty(_customerDetailsViewModel._onBoardCustomerDTO.StoreAdditionalInfoCMI.AodaExpectedTo1.ToString()) ||
                    string.IsNullOrEmpty(_customerDetailsViewModel._onBoardCustomerDTO.StoreAdditionalInfoCMI.AodaExpectedTo2.ToString()) ||
                    string.IsNullOrEmpty(_customerDetailsViewModel._onBoardCustomerDTO.StoreAdditionalInfoCMI.AodaExpectedTo3.ToString());

                if (hasErrors)
                {
                    sectionsWithErrors.Add("Area of Distribution Agreed");
                }
                else
                {
                    sectionsWithErrors.Remove("Area of Distribution Agreed");
                }

                return await FinalizeValidation();
            }
            catch (Exception ex)
            {
                ValidationMessage = $"An error occurred: {ex.Message}";
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public async Task<bool> ValidateAreaofOperationAgreed()
        {
            try
            {
                bool hasErrors =
                    string.IsNullOrEmpty(_customerDetailsViewModel._onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaCode) ||
                    string.IsNullOrEmpty(_customerDetailsViewModel._onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaCcFcContactPerson) ||
                    string.IsNullOrEmpty(_customerDetailsViewModel._onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaCcFcReplyReceivedBy);

                if (hasErrors)
                {
                    sectionsWithErrors.Add("Area of Operation Agreed");
                }
                else
                {
                    sectionsWithErrors.Remove("Area of Operation Agreed");
                }

                return await FinalizeValidation();
            }
            catch (Exception ex)
            {
                ValidationMessage = $"An error occurred: {ex.Message}";
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public async Task<bool> ValidateDocumentAppendix()
        {
            try
            {
                if (await ValidateDocuments())
                {
                    sectionsWithErrors.Add("Document Appendix");
                }
                else
                {
                    sectionsWithErrors.Remove("Document Appendix");
                }

                return await FinalizeValidation();
            }
            catch (Exception ex)
            {
                ValidationMessage = $"An error occurred: {ex.Message}";
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public async Task<bool> ValidateDocuments()
        {
            try
            {
                if (!_customerDetailsViewModel._onBoardCustomerDTO.FileSys.Any(p => p.FileSysType == FileSysTypeConstants.GST))
                {
                    if (_customerDetailsViewModel.DocumentAppendixfileSysList.Any())
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<bool> ValidateCodeofEthics()
        {
            try
            {
                if (!await CheckCodeOfEthics())
                {
                    sectionsWithErrors.Add("Code of Ethics");
                }
                else
                {
                    sectionsWithErrors.Remove("Code of Ethics");
                }

                return await FinalizeValidation();
            }
            catch (Exception ex)
            {
                ValidationMessage = $"An error occurred: {ex.Message}";
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public async Task<bool> CheckCodeOfEthics()
        {
            if (!_customerDetailsViewModel.IsCodeOfEthics)
            {
                return _codeEthicsConductPartners.fileSysList.Where(s => s.FileSysType == Winit.Modules.Store.Model.Constants.FileSysTypeConstants.CodeEthicsConductPartners).Any();
            }
            else
            {
                return _customerDetailsViewModel.IsCodeOfEthics;
            }
        }
        public async Task<bool> ValidateTermsAndConditions()
        {
            try
            {
                if (!_customerDetailsViewModel._onBoardCustomerDTO.StoreAdditionalInfoCMI.IsTcAgreed)
                {
                    sectionsWithErrors.Add("Terms And Conditions");
                }
                else
                {
                    sectionsWithErrors.Remove("Terms And Conditions");
                }

                return await FinalizeValidation();
            }
            catch (Exception ex)
            {
                ValidationMessage = $"An error occurred: {ex.Message}";
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private async Task AsmAssign()
        {
            try
            {
                IsAsm = !IsAsm;
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task AssignToAsm()
        {
            try
            {
                if (!string.IsNullOrEmpty(Store.ReportingEmpUID))
                {
                    Store.UID = _customerDetailsViewModel.StoreUID;
                    Store.Status = SalesOrderStatus.PENDING_FROM_ASM;
                    StoreApprovalDTO storeApprovalDTO = new StoreApprovalDTO();
                    storeApprovalDTO.Store = Store;
                    _loadingService.ShowLoading();
                    if (await _customerDetailsViewModel.SaveUpdateDistDetails(storeApprovalDTO))
                    {
                        _tost.Add("Data", "Data Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                        _navigationManager.NavigateTo("CustomerDetails");
                    }
                    else
                    {
                        ShowErrorSnackBar("Error", "Failed to save...");
                        await JSRuntime.InvokeVoidAsync("focustotable", "top");

                    }
                    _loadingService.HideLoading();
                }
                else
                {
                    _tost.Add("Alert", "Please select Asm", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                }
            }
            catch (Exception ex)
            {
                _tost.Add("Alert", "Exception", Winit.UIComponents.SnackBar.Enum.Severity.Error);
            }
        }
        public async Task AsmSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                Store.ReportingEmpUID = selecetedValue?.UID;
                StateHasChanged();
            }
        }
        //private async Task<bool> FinalizeValidation()
        //{
        //    if (sectionsWithErrors.Any())
        //    {
        //        ValidationMessage = $"Please fill the mandatory sections: {string.Join(", ", sectionsWithErrors)}";
        //        return false;
        //    }
        //    else
        //    {
        //        ValidationMessage = string.Empty;
        //        Console.WriteLine("Validation successful");
        //        return true;
        //    }
        //}
        #region Edit after final Approval(ChangeRequestLogic)

        public async Task InsertDataInChangeRequest(string changeRecordDTOJson)
        {
            ShowLoader();
            if (await _customerDetailsViewModel.InsertDataInChangeRequest(changeRecordDTOJson))
            {
                if (_customerDetailsViewModel.CustomerEditApprovalRequired)
                {
                    RequestId = 0;
                    //await executeRule();
                }
            }
            HideLoader();
        }
        #endregion
    }
}

