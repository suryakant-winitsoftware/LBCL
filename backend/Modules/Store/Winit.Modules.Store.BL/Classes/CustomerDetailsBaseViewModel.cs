using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Winit.Modules.Address.Model.Interfaces;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.BroadClassification.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Contact.Model.Constants;
using Winit.Modules.Contact.Model.Interfaces;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.FileSys.Model.Classes;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Location.Model.Constants;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Constants;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common.GST;
using WINITSharedObjects.Constants;

namespace Winit.Modules.Store.BL.Classes
{
    public abstract class CustomerDetailsBaseViewModel : ICustomerDetailsViewModel
    {
        public string NewlyGeneratedUID { get; set; }
        public bool IsLogin { get; set; }
        public bool IsMobile { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public List<FilterCriteria> FilterCriterias { get; set; } = new List<FilterCriteria>();
        public List<SortCriteria> SortCriterias { get; set; }
        public Dictionary<string, List<string>> UserRole_Code { get; set; }
        public List<GSTINDetailsModel> ShipToAddressFromGst { get; set; }
        public GSTINDetailsModel BillToAddressFromGst { get; set; }
        public int TotalCount { get; set; } = 0;
        public List<IContact> ContactList { get; set; }
        public List<IAddress> AddressList { get; set; }
        public string CustomerCode { get; set; }
        public Winit.Modules.Store.Model.Classes.Store SelfUID { get; set; }
        public IContact Contact { get; set; }
        public IAddress Address { get; set; }
        public bool IsEditOnBoardDetails { get; set; }
        public IOnBoardCustomerDTO OnBoardCustomerDTO { get; set; }
        public string BroadClassfication { get; set; }
        public Winit.Modules.Setting.BL.Interfaces.IAppSetting _appSetting;
        public string UserCode { get; set; }
        public string SelfRegistrationUID { get; set; }
        public string UserType { get; set; }
        public string UserRoleCode { get; set; }
        public bool IsNew { get; set; }
        public int RuleId { get; set; }
        public GSTINDetailsModel GSTINDetails { get; set; } = new GSTINDetailsModel();
        public string StoreAdditionalInfoCMIUid { get; set; }
        public string StoreUID { get; set; }
        public bool IsRejectButtonNeeded { get; set; }
        public bool CustomerEditApprovalRequired { get; set; }
        public bool IsCodeOfEthics { get; set; }
        public string CustomerCreationStoreUID { get; set; }
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Common.BL.Interfaces.IAppUser _appuser;
        private Winit.Modules.Base.BL.ApiService _apiService;
        public string TabName { get; set; }

        //Gridview
        public List<IOnBoardGridview> onBoardGridviewList { get; set; }
        public List<IAllApprovalRequest> AllApprovalLevelList { get; set; }
        //customer information
        public IOnBoardCustomerDTO? _onBoardCustomerDTO { get; set; }
        public List<List<IFileSys>>? DocumentAppendixfileSysList { get; set; } = new List<List<IFileSys>>();
        public IOnBoardCustomerDTO? _originalOnBoardCustomerDTO { get; set; }
        public List<ISelectionItem> CustomerClassificationselectionItems { get; set; }
        public List<ISelectionItem> CustomerClassificationselectionItemsByRoleId { get; set; }
        public List<ISelectionItem> ClassificationTypeselectionItems { get; set; }
        public List<ISelectionItem> FirmTypeselectionItems { get; set; } = new List<ISelectionItem>
        {
            new SelectionItem { UID = "1", Label = FirmTypeConstants.Propertier},
            new SelectionItem { UID = "2", Label = FirmTypeConstants.Partnership},
            new SelectionItem { UID = "3", Label = FirmTypeConstants.Company },

            };
        public List<ISelectionItem> WayOfCommunication { get; set; } = new List<ISelectionItem>
        {
            new SelectionItem { UID = "1", Label = "Telephonic"},
            new SelectionItem { UID = "2", Label = "Video call"},
        };

        public List<ISelectionItem> AooaTypeselectionItems { get; set; } = new List<ISelectionItem>
        {
            new SelectionItem { UID = "1", Label = "ASP"},
            new SelectionItem { UID = "2", Label = "ASP Plus"},
            new SelectionItem { UID = "3", Label = "Installation Partner" },

            };
        //contact details
        public IContact SelectedContact { get; set; }
        public List<IContact> Contacts { get; set; } = new List<IContact>();
        public IAddress SelectedBillToAddress { get; set; } = new Winit.Modules.Address.Model.Classes.Address();
        public IAddress SelectedShipToAddress { get; set; } = new Winit.Modules.Address.Model.Classes.Address();
        public List<IAddress> ShipToAddresses { get; set; } = new List<IAddress>();
        public List<IAddress> OriginalShipToAddresses { get; set; }
        public List<IAddress> BillToAddresses { get; set; } = new List<IAddress>();
        public List<IAddress> OriginalBillToAddresses { get; set; }
        public IStoreShowroom SelectedShowRoom { get; set; } = new Winit.Modules.Store.Model.Classes.StoreShowroom();
        public IStoreBanking SelectedBanking { get; set; } = new StoreBanking();
        public IStoreBanking OriginalSelectedBanking { get; set; }
        public List<IStoreBankingJson> SelectedBankingJson { get; set; } = new List<IStoreBankingJson>();
        public IStoreAdditionalInfoCMI storeAdditionalInfoCMI { get; set; } = new StoreAdditionalInfoCMI();
        public List<StoreAdditionalInfoCMIRetailingCityMonthlySales1> RetailingCityMonthlySalesList { get; set; } = new List<StoreAdditionalInfoCMIRetailingCityMonthlySales1>();
        public List<StoreAdditionalInfoCMIRetailingCityMonthlySales1> OriginalRetailingCityMonthlySalesList { get; set; }
        public List<StoreAdditionalInfoCMIRACSalesByYear1> RACSalesByYearList { get; set; } = new List<StoreAdditionalInfoCMIRACSalesByYear1>();
        public List<StoreAdditionalInfoCMIRACSalesByYear1> OriginalRACSalesByYearList { get; set; }
        public IStoreAdditionalInfoCMIRetailingCityMonthlySales RetailingCityMonthlySales { get; set; } = new StoreAdditionalInfoCMIRetailingCityMonthlySales();
        public IStoreAdditionalInfoCMIRetailingCityMonthlySales OriginalRetailingCityMonthlySales { get; set; }
        public IStoreAdditionalInfoCMIRACSalesByYear RACSalesByYear { get; set; } = new StoreAdditionalInfoCMIRACSalesByYear();
        public IStoreAdditionalInfoCMIRACSalesByYear OriginalRACSalesByYear { get; set; }
        public List<StoreBrandDealingIn> brandInfos { get; set; } = new List<StoreBrandDealingIn> { new StoreBrandDealingIn { Sn = 1 } };
        public List<NameAndAddressOfProprietorModel> KartaEditDetails { get; set; } = new List<NameAndAddressOfProprietorModel> { new NameAndAddressOfProprietorModel { Sn = 1 } };
        public List<StoreBrandDealingIn> OriginalBrandInfos { get; set; }
        public List<SupervisorInfo> SupervisorInfos { get; set; } = new List<SupervisorInfo> { new SupervisorInfo { Sn = 1 } };
        public List<SupervisorInfo> OriginalSupervisorInfos { get; set; }
        public List<TechniciansInfo> TechnicianInfo { get; set; } = new List<TechniciansInfo> { new TechniciansInfo { Sn = 1 } };
        public List<TechniciansInfo> OriginalTechnicianInfo { get; set; }
        public IStoreAdditionalInfoCMI StoreAdditionalInfoCMI { get; set; } = new StoreAdditionalInfoCMI();
        public List<IStoreShowroom> ShowroomDetails { get; set; } = new List<IStoreShowroom>();
        public List<IStoreShowroom> OriginalShowroomDetails { get; set; }
        public IStoreSignatory storeSignatory { get; set; } = new StoreSignatory();
        public IStoreSignatory OriginalstoreSignatory { get; set; }
        public List<IAllApprovalRequest> ApprovalStatus { get; set; } = new List<IAllApprovalRequest>();
        public List<IStoreSignatory> SignatoryDetails { get; set; } = new List<IStoreSignatory>();
        public List<IStoreSignatory> OriginalSignatoryDetails { get; set; }
        public List<ISelectionItem> StateselectionItems { get; set; }
        public ILocation CountryRegionSelectionItems { get; set; } = new Winit.Modules.Location.Model.Classes.Location();
        public List<ISelectionItem> CityselectionItems { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> LocalityselectionItems { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> BranchselectionItems { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> AllASMselectionItems { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> DivisionMappingselectionItems { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> AsmMappingselectionItems { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> ASEMMappingselectionItems { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> ASMselectionItems { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> OUselectionItems { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> DivisionSelectionItems { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> OUselectionItemsForShipTo { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> SalesOfficeselectionItems { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> PinCodeselectionItems { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> StateselectionItemsForShipTo { get; set; }
        public List<ISelectionItem> CityselectionItemsForShipTo { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> LocalityselectionItemsForShipTo { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> StateselectionItemsForShowRoom { get; set; }
        public List<ISelectionItem> CityselectionItemsForShowRoom { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> LocalityselectionItemsForShowRoom { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> PinCodeselectionItemsForShowRoom { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> BranchselectionItemsForShowRoom { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> BranchselectionItemsForShipTo { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> SalesOfficeselectionItemsForShipTo { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> ASMselectionItemsForShipTo { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> ASEMselectionItemsForShipTo { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> PinCodeselectionItemsForShipTo { get; set; } = new List<ISelectionItem>();
        public List<IAsmDivisionMapping> AsmDivisionMappingDetails { get; set; } = new List<IAsmDivisionMapping>();
        public List<IAsmDivisionMapping> AsmMappingGridDetails { get; set; } = new List<IAsmDivisionMapping>();
        public List<ISelectionItem> DivisionDetails { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> ContactTypeSelectionItems { get; set; } = new List<ISelectionItem>
            {
                new SelectionItem { UID = "1", Label = ContactType.Office },
                new SelectionItem { UID = "2", Label = ContactType.Residence }
            };
        //Address details
        public List<ISelectionItem> GenderselectionItems { get; set; } = new List<ISelectionItem>
{
    new SelectionItem { UID = "1", Label = "Male"},
    new SelectionItem { UID = "2", Label = "Female"},
    new SelectionItem { UID = "3", Label = "Other" },

};
        public List<string> State { get; set; } = new List<string> { "State" };
        public IOnBoardEditCustomerDTO EditOnBoardingDetails { get; set; } = new OnBoardEditCustomerDTO();

        public Winit.Modules.Contact.Model.Classes.Contact? _originalContact { get; set; }
        public ISelfRegistration selfRegistration { get; set; }
        public List<List<IFileSys>> DocumentFilesys { get; set; }
        public string CurrentStatus { get; set; } = "Draft";
        public SortCriteria DefaultSortCriteria = new("ModifiedTime", SortDirection.Desc);

        public CustomerDetailsBaseViewModel(IServiceProvider serviceProvider,
           IFilterHelper filter,
           ISortHelper sorter,
           IAppUser appuser,
           IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService,
            Winit.Modules.Setting.BL.Interfaces.IAppSetting appSetting)
        {
            try
            {
                _serviceProvider = serviceProvider;
                _filter = filter;
                _sorter = sorter;
                _listHelper = listHelper;
                _apiService = apiService;
                _appConfigs = appConfigs;
                _appuser = appuser;
                _appSetting = appSetting;
                ShipToAddressFromGst = new List<GSTINDetailsModel>();
                BillToAddressFromGst = new GSTINDetailsModel();
                ContactList = new List<IContact>();
                AddressList = new List<IAddress>();
                Contact = new Winit.Modules.Contact.Model.Classes.Contact();
                Address = new Winit.Modules.Address.Model.Classes.Address();
                onBoardGridviewList = new List<IOnBoardGridview>();
                AllApprovalLevelList = new List<IAllApprovalRequest>();
                CustomerClassificationselectionItems = new List<ISelectionItem>();
                ClassificationTypeselectionItems = new List<ISelectionItem>();
                StateselectionItems = new List<ISelectionItem>();
                StateselectionItemsForShipTo = new List<ISelectionItem>();
                UserCode = _appuser?.Emp?.Code ?? "";
                UserRoleCode = _appuser?.Role?.Code ?? "";
                UserType = _appuser?.SelectedJobPosition?.OrgUID ?? "";
                IsRejectButtonNeeded = _appSetting.IsRejectNeededInOnBoarding;
                CustomerEditApprovalRequired = _appSetting.Customer_Edit_Approval_Required;
                selfRegistration = new SelfRegistration();
                DocumentFilesys = new List<List<IFileSys>>();
                SortCriterias = new List<SortCriteria>();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task PageIndexChanged(int pageNumber)
        {
            PageNumber = pageNumber;
            await PopulateViewModel();
        }
        public async Task OnSorting(SortCriteria sortCriteria)
        {
            try
            {
                SortCriterias.Clear();
                SortCriterias.Add(sortCriteria);
                await PopulateViewModel();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public virtual async Task PopulateViewModel()
        {
            onBoardGridviewList.Clear();
            FilterCriteria? filterCriteria = FilterCriterias.Find(e => e.Name == "Status");
            if (filterCriteria != null)
            {
                FilterCriterias.Remove(filterCriteria);
            }
            if (TabName == "Draft")
            {
                List<string> list = new List<string>
                    {
                        "Draft", SalesOrderStatus.PENDING_FROM_BM
                    };
                FilterCriterias.Add(new FilterCriteria("Status", list, FilterType.In));
            }
            if (TabName == "Assigned")
            {
                FilterCriterias.Add(new FilterCriteria("Status", "Pending", FilterType.Equal));
            }
            if (TabName == "Confirmed")
            {
                FilterCriterias.Add(new FilterCriteria("Status", "Approved", FilterType.Equal));
            }
            if (TabName == "Rejected")
            {
                FilterCriterias.Add(new FilterCriteria("Status", TabName, FilterType.Equal));
            }
            if (TabName == SalesOrderStatus.PENDING_FROM_ASM || TabName == "Pending from Asm")
            {
                FilterCriterias.Add(new FilterCriteria("Status", TabName, FilterType.Equal));
            }
            onBoardGridviewList.AddRange(await GetOnBoardDetailsGridiview() ?? new());
        }
        public async Task OnFilterApply(Dictionary<string, string> keyValuePairs)
        {
            try
            {
                FilterCriterias.Clear();
                foreach (var keyValue in keyValuePairs)
                {
                    if (!string.IsNullOrEmpty(keyValue.Value))
                    {
                        if (keyValue.Value.Contains(","))
                        {
                            string[] values = keyValue.Value.Split(',');
                            FilterCriterias.Add(new FilterCriteria(keyValue.Key, values, FilterType.In));
                        }
                        else
                        {
                            FilterCriterias.Add(new FilterCriteria(keyValue.Key, keyValue.Value, FilterType.Like));
                        }
                    }
                }
                await PopulateViewModel();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task GetStates()
        {
            try
            {
                StateselectionItems = CommonFunctions.ConvertToSelectionItems(await GetStateDetails(State), new List<string> { "UID", "Code", "Name" });
                OUselectionItems = CommonFunctions.ConvertToSelectionItems(await GetOUDetails(StoreConstants.OU), new List<string> { "UID", "Code", "Name" });
                OUselectionItemsForShipTo = CommonFunctions.ConvertToSelectionItems(await GetOUDetails(StoreConstants.OU), new List<string> { "UID", "Code", "Name" });
            }
            catch (Exception ex)
            {

            }
        }
        public async Task GetCountryAndRegionByState(string UID, string Type)
        {
            try
            {
                CountryRegionSelectionItems = await GetCountryAndRegionDetails(UID, Type);
            }
            catch (Exception ex)
            {

            }
        }
        public async Task GetStatesForShipTo()
        {
            try
            {
                StateselectionItemsForShipTo = CommonFunctions.ConvertToSelectionItems(await GetStateDetails(State), new List<string> { "UID", "Code", "Name" });
            }
            catch (Exception ex)
            {

            }
        }
        public async Task GetStatesForShowRoom()
        {
            try
            {
                StateselectionItemsForShowRoom = CommonFunctions.ConvertToSelectionItems(await GetStateDetails(State), new List<string> { "UID", "Code", "Name" });
            }
            catch (Exception ex)
            {

            }
        }
        public async Task<List<IAllApprovalRequest>> GetApprovalStatusDetails(string LinkItemUID)
        {
            try
            {
                ApprovalStatus = await GetApprovalStatus(LinkItemUID);
                return ApprovalStatus;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public virtual async Task<List<IContact>> GetAllContacts()
        {
            ContactList.Clear();
            ContactList.AddRange(await GetAllContactData());
            return ContactList;
        }
        public void GenerateCustomerCode()
        {
            CustomerCode = DateTime.Now.ToString("yyyyMMddHHmmss");
        }
        public async Task<Winit.Modules.Store.Model.Classes.Store> CheckStoreExistsOrNot(string UID)
        {
            SelfUID = await CheckExistOrNot(UID);
            return SelfUID;
        }
        public virtual async Task<List<IAddress>> GetAllAddress(string Type)
        {
            AddressList.Clear();
            AddressList.AddRange(await GetAllAddressData(Type));
            return AddressList;
        }

        public async Task<bool> SaveUpdateContact(IContact contact, bool Iscreate)
        {
            if (Iscreate)
            {
                contact.UID = Guid.NewGuid().ToString();
                contact.ServerAddTime = DateTime.Now;
                contact.ServerModifiedTime = DateTime.Now;
                contact.LinkedItemType = "Store";
                contact.LinkedItemUID = StoreUID;
                NewlyGeneratedUID = contact.UID;
                return await CreateUpdatecontact(contact, true);
            }
            else
            {
                return await CreateUpdatecontact(contact, false);

            }
        }


        public async Task<bool> SaveUpdateCustomerInformation(IOnBoardCustomerDTO onBoardCustomerDTO, bool iscreate)
        {
            try
            {
                if (iscreate)
                {
                    if (SelfRegistrationUID == null)
                    {
                        onBoardCustomerDTO.Store.UID = Guid.NewGuid().ToString();
                        onBoardCustomerDTO.Store.ReportingEmpUID = _appuser?.Emp?.UID ?? "ADMIN";
                    }
                    else
                    {
                        onBoardCustomerDTO.Store.UID = SelfRegistrationUID;
                        onBoardCustomerDTO.Store.ReportingEmpUID = string.Empty;
                    }
                    onBoardCustomerDTO.StoreAdditionalInfo.UID = Guid.NewGuid().ToString();
                    onBoardCustomerDTO.StoreAdditionalInfoCMI.UID = Guid.NewGuid().ToString();
                    onBoardCustomerDTO.Store.ServerAddTime = DateTime.Now;
                    onBoardCustomerDTO.StoreAdditionalInfo.ServerAddTime = DateTime.Now;
                    onBoardCustomerDTO.StoreAdditionalInfoCMI.ServerAddTime = DateTime.Now;
                    onBoardCustomerDTO.Store.ServerModifiedTime = DateTime.Now;
                    onBoardCustomerDTO.StoreAdditionalInfo.ServerModifiedTime = DateTime.Now;
                    //onBoardCustomerDTO.StoreAdditionalInfoCMI.ServerModifiedTime = DateTime.Now;
                    onBoardCustomerDTO.StoreAdditionalInfoCMI.SelfRegistrationUID = SelfRegistrationUID ?? "";
                    onBoardCustomerDTO.Store.CreatedBy = _appuser?.Emp?.UID ?? "ADMIN";
                    onBoardCustomerDTO.Store.ModifiedBy = _appuser?.Emp?.UID ?? "ADMIN";
                    onBoardCustomerDTO.Store.CreatedTime = DateTime.Now;
                    onBoardCustomerDTO.Store.ModifiedTime = DateTime.Now;
                    onBoardCustomerDTO.Store.Code = CustomerCode;
                    onBoardCustomerDTO.Store.TaxDocNumber = GSTINDetails.GSTIN;
                    onBoardCustomerDTO.Store.IsTaxApplicable = true;
                    onBoardCustomerDTO.Store.StateUID = GSTINDetails.PR_ADR_State;
                    onBoardCustomerDTO.Store.LegalName = GSTINDetails.LegalName;
                    onBoardCustomerDTO.Store.Name = GSTINDetails.TradeName;
                    onBoardCustomerDTO.Store.GSTNo = GSTINDetails.GSTIN;
                    onBoardCustomerDTO.Store.Type = StoreType.FR;
                    onBoardCustomerDTO.Store.Status = SalesOrderStatus.DRAFT;
                    onBoardCustomerDTO.Store.IsTaxApplicable = true;
                    onBoardCustomerDTO.StoreAdditionalInfo.GSTINStatus = GSTINDetails.Status;
                    //onBoardCustomerDTO.StoreAdditionalInfo.PAN = GSTINDetails.pan;
                    onBoardCustomerDTO.StoreAdditionalInfo.OwnerName = GSTINDetails.TradeName;
                    onBoardCustomerDTO.StoreAdditionalInfo.NatureOfBusiness = GSTINDetails.PR_NatureOfBusiness;
                    onBoardCustomerDTO.StoreAdditionalInfo.PinCode = GSTINDetails.PR_ADR_Pincode;
                    onBoardCustomerDTO.StoreAdditionalInfo.DateOfRegistration = GSTINDetails.RegistrationDate;
                    onBoardCustomerDTO.StoreAdditionalInfo.TaxPaymentType = GSTINDetails.Duty;
                    onBoardCustomerDTO.StoreAdditionalInfo.GSTState = GSTINDetails.PR_ADR_State;
                    //onBoardCustomerDTO.StoreAdditionalInfo.GSTAddress = GSTINDetails.PR_ADR_Street;
                    onBoardCustomerDTO.StoreAdditionalInfo.CreatedBy = _appuser?.Emp?.UID ?? "ADMIN";
                    onBoardCustomerDTO.StoreAdditionalInfo.ModifiedBy = _appuser?.Emp?.UID ?? "ADMIN";
                    onBoardCustomerDTO.StoreAdditionalInfo.CreatedTime = DateTime.Now;
                    onBoardCustomerDTO.StoreAdditionalInfo.ModifiedTime = DateTime.Now;
                    onBoardCustomerDTO.StoreAdditionalInfoCMI.CreatedBy = _appuser?.Emp?.UID ?? "ADMIN";
                    onBoardCustomerDTO.StoreAdditionalInfoCMI.ModifiedBy = _appuser?.Emp?.UID ?? "ADMIN";
                    onBoardCustomerDTO.StoreAdditionalInfoCMI.CreatedTime = DateTime.Now;
                    onBoardCustomerDTO.StoreAdditionalInfoCMI.ModifiedTime = DateTime.Now;
                    onBoardCustomerDTO.StoreAdditionalInfo.StoreUID = onBoardCustomerDTO.Store.UID;
                    onBoardCustomerDTO.StoreAdditionalInfoCMI.StoreUid = onBoardCustomerDTO.Store.UID;
                    await MapStoreCredit(onBoardCustomerDTO);
                    onBoardCustomerDTO.FileSys.ForEach(e => e.LinkedItemUID = onBoardCustomerDTO.Store.UID);
                    StoreAdditionalInfoCMIUid = onBoardCustomerDTO.StoreAdditionalInfoCMI.UID;
                    StoreUID = onBoardCustomerDTO.Store.UID;

                    return await CreateUpdateCustomerInformation(onBoardCustomerDTO, true);
                }
                else
                {
                    StoreAdditionalInfoCMIUid = onBoardCustomerDTO.StoreAdditionalInfoCMI.UID;
                    return await CreateUpdateCustomerInformation(onBoardCustomerDTO, false);

                }

            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public async Task MapStoreCredit(IOnBoardCustomerDTO onBoardCustomerDTO)
        {
            try
            {
                onBoardCustomerDTO.StoreCredit.ForEach(p =>
                {
                    p.UID = Guid.NewGuid().ToString();
                    p.StoreUID = onBoardCustomerDTO.Store.UID;
                    p.PaymentTermUID = null;
                    p.CreditLimit = 0;
                    p.TemporaryCredit = 0;
                    p.OrgUID = null;
                    p.DistributionChannelUID = "DC";
                    p.PreferredPaymentMode = "Cash";
                    p.IsActive = true;
                    p.IsBlocked = false;
                    p.BlockingReasonCode = "0";
                    p.BlockingReasonDescription = null;
                    p.CreditDays = 0;
                    p.TemporaryCreditDays = 0;
                    p.TemporaryCreditApprovalDate = DateTime.Now;
                    p.CreatedBy = onBoardCustomerDTO.Store.CreatedBy;
                    p.CreatedTime = onBoardCustomerDTO.Store.CreatedTime;
                    p.ModifiedBy = onBoardCustomerDTO.Store.ModifiedBy;
                    p.ModifiedTime = onBoardCustomerDTO.Store.ModifiedTime;
                    p.ServerAddTime = onBoardCustomerDTO.Store.ServerAddTime;
                    p.ServerModifiedTime = onBoardCustomerDTO.Store.ServerModifiedTime;
                });
            }
            catch (Exception ex)
            {

            }
        }
        public async Task<bool> SaveUpdateBillToAddress(IAddress address, bool Iscreate)
        {
            if (Iscreate)
            {
                address.UID = Guid.NewGuid().ToString();
                CountryRegionSelectionItems = await GetCountryAndRegionDetails(StateselectionItems.Where(p => p.IsSelected).FirstOrDefault().UID, "Country");
                address.ServerAddTime = DateTime.Now;
                address.ServerModifiedTime = DateTime.Now;
                address.Type = "Billing";
                address.LinkedItemType = LinkedItemType.STORE;
                address.LinkedItemUID = StoreUID;
                address.CountryCode = CountryRegionSelectionItems.Code;
                CountryRegionSelectionItems = await GetCountryAndRegionDetails(StateselectionItems.Where(p => p.IsSelected).FirstOrDefault().UID, "Region");
                address.RegionCode = CountryRegionSelectionItems.Code;
                NewlyGeneratedUID = address.UID;
                return await CreateUpdateaddress(address, true);
            }
            else
            {
                return await CreateUpdateaddress(address, false);
            }
        }
        public async Task<bool> SaveUpdateAsmDivision(List<IAsmDivisionMapping> asmDivisionMapping, bool Iscreate)
        {
            asmDivisionMapping.ForEach(p =>
                {
                    if (string.IsNullOrEmpty(p.UID))
                    {
                        p.UID = Guid.NewGuid().ToString();
                    }
                });
            return await CreateUpdateAsmDivisionMapping(asmDivisionMapping, true);
        }
        public async Task<bool> SaveUpdateShipToAddress(IAddress address, bool Iscreate)
        {
            if (Iscreate)
            {
                address.ServerAddTime = DateTime.Now;
                address.ServerModifiedTime = DateTime.Now;
                address.Type = "Shipping";
                address.LinkedItemType = LinkedItemType.STORE;
                address.LinkedItemUID = StoreUID;
                CountryRegionSelectionItems = await GetCountryAndRegionDetails(StateselectionItemsForShipTo.Where(p => p.IsSelected).FirstOrDefault().UID, "Country");
                address.CountryCode = CountryRegionSelectionItems.Code;
                CountryRegionSelectionItems = await GetCountryAndRegionDetails(StateselectionItemsForShipTo.Where(p => p.IsSelected).FirstOrDefault().UID, "Region");
                address.RegionCode = CountryRegionSelectionItems.Code;
                return await CreateUpdateaddress(address, true);
            }
            else
            {
                return await CreateUpdateaddress(address, false);

            }

        }
        public async Task GetRuleId(string Type, string TypeCode)
        {
            await GetRuleIdData(Type, TypeCode);
        }

        public async Task GetAllUserHierarchy(string hierarchyType, string hierarchyUID, int ruleId)
        {
            await GetAllUserHierarchyData(hierarchyType, hierarchyUID, ruleId);

        }
        public async Task<bool> SaveAllApprovalRequestDetails(string RequestId)
        {
            return await SaveApprovalRequestDetails(RequestId);
        }
        public async Task<bool> SaveUpdateEmployeeDetails(IStoreAdditionalInfoCMI storeAdditionalInfoCMI)
        {
            //if (Iscreate)
            //{
            //    storeAdditionalInfoCMI.UID = Guid.NewGuid().ToString();
            //    storeAdditionalInfoCMI.ServerAddTime = DateTime.Now;
            //    storeAdditionalInfoCMI.ServerModifiedTime = DateTime.Now;
            //   // storeAdditionalInfoCMI.StoreUid = OnBoardCustomerDTO.Store.UID;
            //    return await CreateUpdateEmployeeDetails(storeAdditionalInfoCMI, true);
            //}
            //else
            //{
            return await CreateUpdateEmployeeDetails(storeAdditionalInfoCMI);

            //}
        }
        public async Task<bool> SaveUpdateDistDetails(StoreApprovalDTO storeApprovalDTO)
        {
            //if (Iscreate)
            //{
            //    storeAdditionalInfoCMI.UID = Guid.NewGuid().ToString();
            //    storeAdditionalInfoCMI.ServerAddTime = DateTime.Now;
            //    storeAdditionalInfoCMI.ServerModifiedTime = DateTime.Now;
            //   // storeAdditionalInfoCMI.StoreUid = OnBoardCustomerDTO.Store.UID;
            //    return await CreateUpdateEmployeeDetails(storeAdditionalInfoCMI, true);
            //}
            //else
            //{

            return await CreateUpdateDistDetails(storeApprovalDTO);

            //} 
        }

        public async Task<bool> InsertDataInChangeRequest(string changeRecordDTOJson)
        {
            return await InsertDataInChangeRequestTable(changeRecordDTOJson);
        }
        public async Task<string> DeleteContactDetails(string UID)
        {
            return await DeleteContactDetailsFromGrid(UID);
        }
        public async Task<string> DeleteAddressDetails(string UID)
        {
            return await DeleteAddressDetailsFromGrid(UID);
        }
        public async Task OnChangeBroadClassification(string UID)
        {
            try
            {
                ClassificationTypeselectionItems = CommonFunctions.ConvertToSelectionItems(await GetBroadClassificationLineDetails(UID), new List<string> { "UID", "ClassificationCode", "ClassificationCode" });
                ClassificationTypeselectionItems = ClassificationTypeselectionItems
                                                    .GroupBy(item => item.Code)
                                                    .Select(group => group.First())
                                                    .ToList();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task AsmOnload()
        {
            try
            {
                List<IStoreCredit> storeCreditDetails = await GetStoreCredit();
                DivisionMappingselectionItems = CommonFunctions.ConvertToSelectionItems(storeCreditDetails.Where(p => p.IsActive).ToList(), new List<string> { "DivisionOrgUID", "StoreUID", "DivisionName" });
                DivisionMappingselectionItems = DivisionMappingselectionItems.Where(p => p.Code == StoreUID).ToList();
                DivisionSelectionItems = DivisionMappingselectionItems.Where(p => p.Code == StoreUID).ToList();
                AsmMappingselectionItems = CommonFunctions.ConvertToSelectionItems(await GetASMDetailsFromBranch(BillToAddresses.Where(p => p.IsDefault).FirstOrDefault().BranchUID, ""), new List<string> { "UID", "Code", "Name" });
                ASEMMappingselectionItems = CommonFunctions.ConvertToSelectionItems(await GetASEMDetailsFromBranch(BillToAddresses.Where(p => p.IsDefault).FirstOrDefault().BranchUID, StoreConstants.ASEM), new List<string> { "UID", "Code", "Name" });
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnStateSelection(List<string> UID)
        {
            try
            {
                CityselectionItems = CommonFunctions.ConvertToSelectionItems(await GetCityAndLocalityDetails(UID), new List<string> { "UID", "Code", "Name" });
                LocalityselectionItems.Clear();
                BranchselectionItems.Clear();
                PinCodeselectionItems.Clear();
                ASMselectionItems.Clear();
                SalesOfficeselectionItems.Clear();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnCitySelection(List<string> UID)
        {
            try
            {
                LocalityselectionItems = CommonFunctions.ConvertToSelectionItems(await GetCityAndLocalityDetails(UID), new List<string> { "UID", "Code", "Name" });
                BranchselectionItems.Clear();
                PinCodeselectionItems.Clear();
                ASMselectionItems.Clear();
                SalesOfficeselectionItems.Clear();
                await OnLocalitySelection(LocalityselectionItems.Select(p => p.UID).Distinct().ToList());
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnLocalitySelection(List<string> UID)
        {
            try
            {
                PinCodeselectionItems.Clear();
                BranchselectionItems.Clear();
                PinCodeselectionItems = CommonFunctions.ConvertToSelectionItems(await GetCityAndLocalityDetails(UID), new List<string> { "UID", "Code", "Name" });
                PinCodeselectionItems = PinCodeselectionItems
                                        .GroupBy(item => item.Code)
                                        .Select(group => group.First())
                                        .ToList();
                List<BranchHeirarchy> SelectedData = new List<BranchHeirarchy>();
                //SelectedData.Add(new BranchHeirarchy { UID = LocalityselectionItems.Where(p => p.IsSelected).First().Code, Name = BranchHeirarchyConstants.Locality });
                SelectedData.Add(new BranchHeirarchy { UID = CityselectionItems.Where(p => p.IsSelected).First().Code, Name = BranchHeirarchyConstants.City });
                SelectedData.Add(new BranchHeirarchy { UID = StateselectionItems.Where(p => p.IsSelected).First().Code, Name = BranchHeirarchyConstants.State });
                foreach (var data in SelectedData)
                {
                    BranchselectionItems = CommonFunctions.ConvertToSelectionItems(await GetBranchDetails(data), new List<string> { "UID", "Code", "Name" });
                    if (BranchselectionItems.Count > 0)
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnBranchSelection(string UID)
        {
            try
            {
                ASMselectionItems.Clear();
                SalesOfficeselectionItems.Clear();
                ASMselectionItems = CommonFunctions.ConvertToSelectionItems(await GetASMDetailsFromBranch(UID, ""), new List<string> { "UID", "Code", "Name" });
                SalesOfficeselectionItems = CommonFunctions.ConvertToSelectionItems(await GetSalesOfficeDetailsFromBranch(UID), new List<string> { "UID", "Code", "Name" });
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnStateSelectionForShipTo(List<string> UID)
        {
            try
            {
                CityselectionItemsForShipTo = CommonFunctions.ConvertToSelectionItems(await GetCityAndLocalityDetails(UID), new List<string> { "UID", "Code", "Name" });
                LocalityselectionItemsForShipTo.Clear();
                BranchselectionItemsForShipTo.Clear();
                PinCodeselectionItemsForShipTo.Clear();
                ASMselectionItemsForShipTo.Clear();
                SalesOfficeselectionItemsForShipTo.Clear();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnCitySelectionForShipTo(List<string> UID)
        {
            try
            {
                LocalityselectionItemsForShipTo = CommonFunctions.ConvertToSelectionItems(await GetCityAndLocalityDetails(UID), new List<string> { "UID", "Code", "Name" });
                BranchselectionItemsForShipTo.Clear();
                PinCodeselectionItemsForShipTo.Clear();
                ASMselectionItemsForShipTo.Clear();
                SalesOfficeselectionItemsForShipTo.Clear();
                await OnLocalitySelectionForShipTo(LocalityselectionItemsForShipTo.Select(p => p.UID).Distinct().ToList<string>());
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnLocalitySelectionForShipTo(List<string> UID)
        {
            try
            {
                BranchselectionItemsForShipTo.Clear();
                PinCodeselectionItemsForShipTo.Clear();
                SalesOfficeselectionItemsForShipTo.Clear();
                PinCodeselectionItemsForShipTo = CommonFunctions.ConvertToSelectionItems(await GetCityAndLocalityDetails(UID), new List<string> { "UID", "Code", "Name" });
                PinCodeselectionItemsForShipTo = PinCodeselectionItemsForShipTo
                        .GroupBy(item => item.Code)
                        .Select(group => group.First())
                        .ToList();
                List<BranchHeirarchy> SelectedData = new List<BranchHeirarchy>();
                //SelectedData.Add(new BranchHeirarchy { UID = LocalityselectionItemsForShipTo.Where(p => p.IsSelected).First().Code, Name = BranchHeirarchyConstants.Locality });
                SelectedData.Add(new BranchHeirarchy { UID = CityselectionItemsForShipTo.Where(p => p.IsSelected).First().Code, Name = BranchHeirarchyConstants.City });
                SelectedData.Add(new BranchHeirarchy { UID = StateselectionItemsForShipTo.Where(p => p.IsSelected).First().Code, Name = BranchHeirarchyConstants.State });
                foreach (var data in SelectedData)
                {
                    BranchselectionItemsForShipTo = CommonFunctions.ConvertToSelectionItems(await GetBranchDetails(data), new List<string> { "UID", "Code", "Name" });
                    if (BranchselectionItemsForShipTo.Count > 0)
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnStateSelectionForShowRoom(List<string> UID)
        {
            try
            {
                CityselectionItemsForShowRoom = CommonFunctions.ConvertToSelectionItems(await GetCityAndLocalityDetails(UID), new List<string> { "UID", "Code", "Name" });
                LocalityselectionItemsForShowRoom.Clear();
                PinCodeselectionItemsForShowRoom.Clear();
                BranchselectionItemsForShowRoom.Clear();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnCitySelectionForShowRoom(List<string> UID)
        {
            try
            {
                LocalityselectionItemsForShowRoom = CommonFunctions.ConvertToSelectionItems(await GetCityAndLocalityDetails(UID), new List<string> { "UID", "Code", "Name" });
                PinCodeselectionItemsForShowRoom.Clear();
                BranchselectionItemsForShowRoom.Clear();
                await OnLocalitySelectionForShowRoom(LocalityselectionItemsForShowRoom.Select(p => p.UID).Distinct().ToList<string>());
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnLocalitySelectionForShowRoom(List<string> UID)
        {
            try
            {
                PinCodeselectionItemsForShowRoom = CommonFunctions.ConvertToSelectionItems(await GetCityAndLocalityDetails(UID), new List<string> { "UID", "Code", "Name" });
                List<BranchHeirarchy> SelectedData = new List<BranchHeirarchy>();
                //SelectedData.Add(new BranchHeirarchy { UID = LocalityselectionItemsForShowRoom.Where(p => p.IsSelected).First().Code, Name = BranchHeirarchyConstants.Locality });
                SelectedData.Add(new BranchHeirarchy { UID = CityselectionItemsForShowRoom.Where(p => p.IsSelected).First().Code, Name = BranchHeirarchyConstants.City });
                SelectedData.Add(new BranchHeirarchy { UID = StateselectionItemsForShowRoom.Where(p => p.IsSelected).First().Code, Name = BranchHeirarchyConstants.State });
                foreach (var data in SelectedData)
                {
                    BranchselectionItemsForShowRoom = CommonFunctions.ConvertToSelectionItems(await GetBranchDetails(data), new List<string> { "UID", "Code", "Name" });
                    if (BranchselectionItemsForShowRoom.Count > 0)
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnBranchSelectionForShipTo(string BranchUID)
        {
            try
            {
                ASMselectionItemsForShipTo.Clear();
                SalesOfficeselectionItemsForShipTo.Clear();
                ASMselectionItemsForShipTo = CommonFunctions.ConvertToSelectionItems(await GetASMDetailsFromBranch(BranchUID, ""), new List<string> { "UID", "Code", "Name" });
                ASEMselectionItemsForShipTo = CommonFunctions.ConvertToSelectionItems(await GetASEMDetailsFromBranch(BranchUID, "ASEM"), new List<string> { "UID", "Code", "Name" });
                SalesOfficeselectionItemsForShipTo = CommonFunctions.ConvertToSelectionItems(await GetSalesOfficeDetailsFromBranch(BranchUID), new List<string> { "UID", "Code", "Name" });
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnOUSelectionForShipTo(string ParentUID)
        {
            //try
            //{
            //    DivisionSelectionItems.Clear();
            //    DivisionSelectionItems = CommonFunctions.ConvertToSelectionItems(await GetDivisionDetails(StoreConstants.Supplier, ParentUID), new List<string> { "UID", "Code", "Name" });
            //}
            //catch (Exception ex)
            //{

            //}
        }
        public async Task PopulateOnBoardDetails(string CustomerID)
        {
            try
            {
                EditOnBoardingDetails = await GetAllOnBoardingDetailsByStoreUID(CustomerID);
                CustomerClassificationselectionItemsByRoleId = CommonFunctions.ConvertToSelectionItems(await GetBroadClassificationDetails(), new List<string> { "UID", "Code", "Name" });
                var requiredCodes = new[] { StoreConstants.Trader, StoreConstants.Service };
                if (_appuser.Role.Code == StoreConstants.ASEM)
                {
                    CustomerClassificationselectionItems = CustomerClassificationselectionItemsByRoleId.Where(x => requiredCodes.Contains(x.UID)).ToList();
                }
                else if(_appuser.Role.Code == StoreConstants.ASM)
                {
                    CustomerClassificationselectionItems = CustomerClassificationselectionItemsByRoleId.Where(x => !requiredCodes.Contains(x.UID)).ToList();
                }
                else
                {
                    CustomerClassificationselectionItems = CustomerClassificationselectionItemsByRoleId;
                }
                await GetDivisionDetails();
                await GetStates();
                await GetStatesForShipTo();
                await GetStatesForShowRoom();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task<List<ISelectionItem>> GetAllAsmByBranchUID(string BranchUID)
        {
            try
            {
                AllASMselectionItems = CommonFunctions.ConvertToSelectionItems(await GetAllASM(BranchUID), new List<string> { "UID", "Code", "Name" });
                return AllASMselectionItems;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<List<ISelectionItem>> GetAllAsemByBranchUID(string BranchUID)
        {
            try
            {
                return CommonFunctions.ConvertToSelectionItems(await GetASEMDetailsFromBranch(BranchUID, StoreConstants.ASEM), new List<string> { "UID", "Code", "Name" });
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task PopulateGstAddress(List<IAddress> addresses)
        {
            foreach (var address in addresses)
            {
                CountryRegionSelectionItems = await GetCountryAndRegionDetails(StateselectionItems.Where(p => p.Code == address.State.ToUpper()).FirstOrDefault().UID, "Country");
                address.CountryCode = CountryRegionSelectionItems.Code;
                CountryRegionSelectionItems = await GetCountryAndRegionDetails(StateselectionItems.Where(p => p.Code == address.State.ToUpper()).FirstOrDefault().UID, "Region");
                address.RegionCode = CountryRegionSelectionItems.Code;
            }
            await CreateGstAddress(addresses);
            //ShipToAddresses = await GetAllAddress("Shipping");
        }
        public async Task CreateInstances()
        {
            try
            {
                _onBoardCustomerDTO = _serviceProvider.CreateInstance<IOnBoardCustomerDTO>();
                _onBoardCustomerDTO.Store = new Winit.Modules.Store.Model.Classes.Store();
                _onBoardCustomerDTO.StoreAdditionalInfo = new StoreAdditionalInfo();
                _onBoardCustomerDTO.StoreAdditionalInfoCMI = new StoreAdditionalInfoCMI();
                _onBoardCustomerDTO.FileSys = new List<IFileSys>();
                _onBoardCustomerDTO.StoreCredit = new List<IStoreCredit>();
                SelectedContact = _serviceProvider.CreateInstance<IContact>();
                SelectedBillToAddress = _serviceProvider.CreateInstance<IAddress>();
                SelectedShipToAddress = _serviceProvider.CreateInstance<IAddress>();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task MapOnBoardDetails()
        {
            try
            {
                StoreAdditionalInfoCMIUid = EditOnBoardingDetails.StoreAdditionalInfoCMI.UID;
                await MapCustomerInformation();
                await MapContactDetails();
                await MapBillToAddressDetails();
                await MapShipToAddressDetails();
                await MapAsmMappingDetails();
                await MapShowRoomDetails();
                await MapServiceCenterDetails();
                await MapBankDetails();
                await MapBussinessDetails();
                await MapKartaDetails();
                await MapBussinessDetailsInCaseOfDistributor();
                await MapEarlierWorkedWithCMIDetails();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task MapCustomerInformation()
        {
            try
            {
                _onBoardCustomerDTO = _serviceProvider.CreateInstance<IOnBoardCustomerDTO>();
                _onBoardCustomerDTO.Store = new Winit.Modules.Store.Model.Classes.Store();
                _onBoardCustomerDTO.StoreAdditionalInfo = new StoreAdditionalInfo();
                _onBoardCustomerDTO.StoreAdditionalInfoCMI = new StoreAdditionalInfoCMI();
                _onBoardCustomerDTO.FileSys = new List<IFileSys>();
                _onBoardCustomerDTO.StoreCredit = new List<IStoreCredit>();
                _onBoardCustomerDTO.StoreAdditionalInfo = EditOnBoardingDetails.StoreAdditionalInfo;
                _onBoardCustomerDTO.Store = EditOnBoardingDetails.Store;
                _onBoardCustomerDTO.StoreAdditionalInfoCMI = EditOnBoardingDetails.StoreAdditionalInfoCMI;
                _onBoardCustomerDTO.StoreCredit = EditOnBoardingDetails.StoreCredit.ToList<IStoreCredit>();
                _onBoardCustomerDTO.FileSys = EditOnBoardingDetails.FileSys.ToList<IFileSys>();
                var selectedBroadClassification = CustomerClassificationselectionItems.Find(e => e.Label == EditOnBoardingDetails.Store.BroadClassification);
                if (selectedBroadClassification != null)
                {
                    selectedBroadClassification.IsSelected = true;
                    BroadClassfication = selectedBroadClassification.Label;
                }
                await OnChangeBroadClassification(selectedBroadClassification.UID);
                var selectedClassificationType = ClassificationTypeselectionItems.Find(e => e.Label == EditOnBoardingDetails.Store.ClassficationType);
                if (selectedClassificationType != null)
                {
                    selectedClassificationType.IsSelected = true;
                }
                var selectedType = FirmTypeselectionItems.Find(e => e.Label == EditOnBoardingDetails.StoreAdditionalInfo.FirmType);
                if (selectedType != null)
                {
                    selectedType.IsSelected = true;
                }
                var selectedTypeWay = WayOfCommunication.Find(e => e.Label == EditOnBoardingDetails.StoreAdditionalInfoCMI.AoaaCcSecWayOfCommu);
                if (selectedTypeWay != null)
                {
                    selectedTypeWay.IsSelected = true;
                }
                var selectedDivisionDetails = DivisionDetails.Where(e => EditOnBoardingDetails.StoreCredit.Any(q => q.DivisionOrgUID == e.UID && q.IsActive)).ToList();
                if (selectedDivisionDetails != null)
                {
                    selectedDivisionDetails.ForEach(p => p.IsSelected = true);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task MapContactDetails()
        {
            try
            {
                Contacts.Add(EditOnBoardingDetails.Contact);
                var selectedContact = ContactTypeSelectionItems.Find(e => e.Label == EditOnBoardingDetails.Contact.Type);
                if (selectedContact != null)
                {
                    selectedContact.IsSelected = true;
                }
            }
            catch (Exception ex)
            {

            }
        }
        //public async Task OnEditSelectionForBillTo(string UID)
        //{
        //    try
        //    {
        //        StateselectionItems.ForEach(p => p.IsSelected = false);
        //        var selectedState = StateselectionItems.Find(e => e.Label == EditOnBoardingDetails.Address.Where(o => o.UID == UID).First().State);
        //        if (selectedState != null)
        //        {
        //            selectedState.IsSelected = true;
        //        }
        //        await OnStateSelection(new List<string> { selectedState.UID });
        //        var selectedCity = CityselectionItems.Find(e => e.Label == EditOnBoardingDetails.Address.Where(o => o.UID == UID).First().City);
        //        if (selectedCity != null)
        //        {
        //            selectedCity.IsSelected = true;
        //        }
        //        await OnCitySelection(new List<string> { selectedCity.UID });
        //        var selectedLocality = LocalityselectionItems.Find(e => e.Label == EditOnBoardingDetails.Address.Where(o => o.UID == UID).First().Locality);
        //        if (selectedLocality != null)
        //        {
        //            selectedLocality.IsSelected = true;
        //        }
        //        await OnLocalitySelection(new List<string> { selectedLocality.UID });
        //        var selectedPinCode = PinCodeselectionItems.Find(e => e.Label == EditOnBoardingDetails.Address.Where(o => o.UID == UID).First().ZipCode);
        //        if (selectedPinCode != null)
        //        {
        //            selectedPinCode.IsSelected = true;
        //        }
        //        var selectedBranch = BranchselectionItems.Find(e => e.Label == EditOnBoardingDetails.Address.Where(o => o.UID == UID).First().BranchUID);
        //        if (selectedBranch != null)
        //        {
        //            selectedBranch.IsSelected = true;
        //        }
        //        OUselectionItems.ForEach(p => p.IsSelected = false);
        //        var selectedOU = OUselectionItems.Find(e => e.UID == EditOnBoardingDetails.Address.Where(o => o.UID == UID).First().OrgUnitUID);
        //        if (selectedOU != null)
        //        {
        //            selectedOU.IsSelected = true;
        //        }
        //        await OnBranchSelection(selectedBranch.UID);
        //        var selectedSalesOffice = SalesOfficeselectionItems.Find(e => e.Label == EditOnBoardingDetails.Address.Where(o => o.UID == UID).First().SalesOfficeUID);
        //        if (selectedSalesOffice != null)
        //        {
        //            selectedSalesOffice.IsSelected = true;
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        public async Task GetDivisionDetails()
        {
            DivisionDetails = CommonFunctions.ConvertToSelectionItems(await GetAllDivisionDetails(), new List<string> { "UID", "Code", "Name" });
        }
        public async Task GetAsmDivisionDetails(string UID)
        {
            AsmDivisionMappingDetails = await GetAsmDetailsByUID("Address", UID);
        }
        public async Task OnEditSelectionForShowRoom(IStoreShowroom storeShowroom)
        {
            try
            {
                var selectedState = StateselectionItemsForShowRoom.Find(e => e.Code == storeShowroom.State);
                if (selectedState != null)
                {
                    selectedState.IsSelected = true;
                }
                await OnStateSelectionForShowRoom(new List<string> { selectedState.UID });
                var selectedCity = CityselectionItemsForShowRoom.Find(e => e.Code == storeShowroom.City);
                if (selectedCity != null)
                {
                    selectedCity.IsSelected = true;
                }
                await OnCitySelectionForShowRoom(new List<string> { selectedCity.UID });
                var selectedLocality = LocalityselectionItemsForShowRoom.Find(e => e.Code == storeShowroom.Locality);
                await OnLocalitySelectionForShowRoom(LocalityselectionItemsForShowRoom.Select(p => p.UID).ToList());
                var selectedPinCode = PinCodeselectionItemsForShowRoom.Find(e => e.Code == storeShowroom.PinCode);
                if (selectedPinCode != null)
                {
                    selectedPinCode.IsSelected = true;
                }
                var selectedBranch = BranchselectionItemsForShowRoom.Find(e => e.UID == storeShowroom.Branch);
                if (selectedBranch != null)
                {
                    selectedBranch.IsSelected = true;
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnEditSelectionForShipTo(IAddress address)
        {
            try
            {
                var selectedState = StateselectionItemsForShipTo.Find(e => e.Code == address.State);
                if (selectedState != null)
                {
                    selectedState.IsSelected = true;
                }
                await OnStateSelectionForShipTo(new List<string> { selectedState.UID });
                var selectedCity = CityselectionItemsForShipTo.Find(e => e.Code == address.City);
                if (selectedCity != null)
                {
                    selectedCity.IsSelected = true;
                }
                await OnCitySelectionForShipTo(new List<string> { selectedCity.UID });
                await OnLocalitySelectionForShipTo(LocalityselectionItemsForShipTo.Select(p => p.UID).ToList());
                var selectedPinCode = PinCodeselectionItemsForShipTo.Find(e => e.Code == address.ZipCode);
                if (selectedPinCode != null)
                {
                    selectedPinCode.IsSelected = true;
                }
                var selectedBranch = BranchselectionItemsForShipTo.Find(e => e.UID == address.BranchUID);
                if (selectedBranch != null)
                {
                    selectedBranch.IsSelected = true;
                }
                await OnBranchSelectionForShipTo(selectedBranch.UID);
                OUselectionItemsForShipTo.ForEach(p => p.IsSelected = false);
                var selectedOUShipTo = OUselectionItemsForShipTo.Find(e => e.Code == address.OrgUnitUID);
                if (selectedOUShipTo != null)
                {
                    selectedOUShipTo.IsSelected = true;
                }
                await OnOUSelectionForShipTo(selectedOUShipTo.UID);
                var selectedSalesOffice = SalesOfficeselectionItemsForShipTo.Find(e => e.UID == address.SalesOfficeUID);
                if (selectedSalesOffice != null)
                {
                    selectedSalesOffice.IsSelected = true;
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnCopyGSTBillTo(GSTINDetailsModel gSTINDetails)
        {
            try
            {
                // Ensure the state and district are not null before calling ToUpper()
                if (gSTINDetails.PR_ADR_State != null)
                {
                    gSTINDetails.PR_ADR_State = gSTINDetails.PR_ADR_State?.ToUpper() ?? "";
                    var selectedState = StateselectionItems?.Find(e => e.Code == gSTINDetails.PR_ADR_State);
                    if (selectedState != null)
                    {
                        selectedState.IsSelected = true;
                        await OnStateSelection(new List<string> { selectedState.UID });
                    }
                }
                if (gSTINDetails.PR_ADR_District != null)
                {
                    gSTINDetails.PR_ADR_District = gSTINDetails.PR_ADR_District?.ToUpper() ?? "";
                    var selectedCity = CityselectionItems?.Find(e => e.Code == gSTINDetails.PR_ADR_District);
                    if (selectedCity != null)
                    {
                        selectedCity.IsSelected = true;
                        await OnCitySelection(new List<string> { selectedCity.UID });
                    }
                }
                else
                {
                    gSTINDetails.PR_ADR_District = string.Empty;
                }
                if (LocalityselectionItems != null)
                {
                    await OnLocalitySelection(LocalityselectionItems.Select(p => p.UID).ToList());
                }
                if (gSTINDetails.PR_ADR_Pincode != null)
                {
                    var selectedPinCode = PinCodeselectionItems?.Find(e => e.Code == gSTINDetails.PR_ADR_Pincode);
                    if (selectedPinCode != null)
                    {
                        selectedPinCode.IsSelected = true;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnEditSelectionForBillTo(IAddress address)
        {
            try
            {
                string State = address.State.ToUpper();
                var selectedState = StateselectionItems.Find(e => e.Code == State);
                if (selectedState != null)
                {
                    selectedState.IsSelected = true;
                }
                await OnStateSelection(new List<string> { selectedState.UID });
                var selectedCity = CityselectionItems.Find(e => e.Code == address.City.ToUpper());
                if (selectedCity != null)
                {
                    selectedCity.IsSelected = true;
                }
                await OnCitySelection(new List<string> { selectedCity.UID });
                var selectedLocality = LocalityselectionItems.Find(e => e.Code == address.Locality);
                await OnLocalitySelection(LocalityselectionItems.Select(p => p.UID).ToList());
                var selectedPinCode = PinCodeselectionItems.Find(e => e.Code == address.ZipCode);
                if (selectedPinCode != null)
                {
                    selectedPinCode.IsSelected = true;
                }
                var selectedBranch = BranchselectionItems.Find(e => e.UID == address.BranchUID);
                if (selectedBranch != null)
                {
                    selectedBranch.IsSelected = true;
                }
                await OnBranchSelection(selectedBranch.UID);
                OUselectionItems.ForEach(p => p.IsSelected = false);
                var selectedOUShipTo = OUselectionItems.Find(e => e.Code == address.OrgUnitUID);
                if (selectedOUShipTo != null)
                {
                    selectedOUShipTo.IsSelected = true;
                }
                var selectedSalesOffice = SalesOfficeselectionItems.Find(e => e.UID == address.SalesOfficeUID);
                if (selectedSalesOffice != null)
                {
                    selectedSalesOffice.IsSelected = true;
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnCopySelectionForShipTo(IAddress address)
        {
            try
            {
                var selectedState = StateselectionItemsForShipTo.Find(e => e.Code == address.State.ToUpper());
                if (selectedState != null)
                {
                    selectedState.IsSelected = true;
                }
                await OnStateSelectionForShipTo(new List<string> { selectedState.UID });
                var selectedCity = CityselectionItemsForShipTo.Find(e => e.Code == address.City.ToUpper());
                if (selectedCity != null)
                {
                    selectedCity.IsSelected = true;
                }
                await OnCitySelectionForShipTo(new List<string> { selectedCity.UID });
                var selectedLocality = LocalityselectionItemsForShipTo.Find(e => e.Code == address.Locality);
                await OnLocalitySelectionForShipTo(LocalityselectionItemsForShipTo.Select(p => p.UID).ToList());
                var selectedPinCode = PinCodeselectionItemsForShipTo.Find(e => e.Code == address.ZipCode);
                if (selectedPinCode != null)
                {
                    selectedPinCode.IsSelected = true;
                }
                var selectedBranch = BranchselectionItemsForShipTo.Find(e => e.UID == address.BranchUID);
                if (selectedBranch != null)
                {
                    selectedBranch.IsSelected = true;
                }
                OUselectionItemsForShipTo.ForEach(p => p.IsSelected = false);
                var selectedOUShipTo = OUselectionItemsForShipTo.Find(e => e.Code == address.OrgUnitUID || e.UID == address.OrgUnitUID);
                if (selectedOUShipTo != null)
                {
                    selectedOUShipTo.IsSelected = true;
                }
                await OnOUSelectionForShipTo(selectedOUShipTo.UID);
                await OnBranchSelectionForShipTo(selectedBranch.UID);
                var selectedSalesOffice = SalesOfficeselectionItemsForShipTo.Find(e => e.UID == address.SalesOfficeUID);
                if (selectedSalesOffice != null)
                {
                    selectedSalesOffice.IsSelected = true;
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task MapBillToAddressDetails()
        {
            try
            {
                BillToAddresses = EditOnBoardingDetails.Address.Where(o => o.Type == "Billing").ToList<IAddress>();
                var selectedContact = GenderselectionItems.Find(e => e.Label == EditOnBoardingDetails.Address.Where(o => o.Type == "Billing").First().Line4);
                if (selectedContact != null)
                {
                    selectedContact.IsSelected = true;
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task MapShipToAddressDetails()
        {
            try
            {
                ShipToAddresses = EditOnBoardingDetails.Address.Where(o => o.Type != "Billing").ToList<IAddress>();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task MapAsmMappingDetails()
        {
            try
            {
                AsmMappingGridDetails = await GetAsmDetailsByUID("Store", EditOnBoardingDetails.Store.UID);
            }
            catch (Exception ex)
            {

            }
        }
        public async Task MapShowRoomDetails()
        {
            try
            {
                ShowroomDetails = JsonConvert.DeserializeObject<List<StoreShowroom>>(EditOnBoardingDetails.StoreAdditionalInfoCMI.ShowroomDetails).ToList<IStoreShowroom>();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task MapServiceCenterDetails()
        {
            try
            {
                TechnicianInfo = new List<TechniciansInfo>();
                SupervisorInfos = new List<SupervisorInfo>();
                TechnicianInfo = JsonConvert.DeserializeObject<List<TechniciansInfo>>(EditOnBoardingDetails.StoreAdditionalInfoCMI.ScTechnicianData);
                SupervisorInfos = JsonConvert.DeserializeObject<List<SupervisorInfo>>(EditOnBoardingDetails.StoreAdditionalInfoCMI.ScSupervisorData);
            }
            catch (Exception ex)
            {

            }
        }
        public async Task MapBankDetails()
        {
            try
            {
                SignatoryDetails = new List<IStoreSignatory>();
                SelectedBanking = _serviceProvider.CreateInstance<IStoreBanking>();
                SelectedBankingJson = new List<IStoreBankingJson>();
                storeSignatory = _serviceProvider.CreateInstance<IStoreSignatory>();
                storeSignatory = JsonConvert.DeserializeObject<List<StoreSignatory>>(EditOnBoardingDetails.StoreAdditionalInfoCMI.SignatoryDetails).First();
                SignatoryDetails = JsonConvert.DeserializeObject<List<StoreSignatory>>(EditOnBoardingDetails.StoreAdditionalInfoCMI.SignatoryDetails).ToList<IStoreSignatory>();
                SelectedBankingJson = JsonConvert.DeserializeObject<List<StoreBankingJson>>(EditOnBoardingDetails.StoreAdditionalInfoCMI.BankDetails).ToList<IStoreBankingJson>();
                for (int i = 0; i < 1; i++)
                {
                    SelectedBanking.Sn = SelectedBankingJson[i].Sn;
                    SelectedBanking.BankAccountNo1 = SelectedBankingJson[i].BankAccountNo;
                    SelectedBanking.BankName1 = SelectedBankingJson[i].BankName;
                    SelectedBanking.BankAddressFirst1 = SelectedBankingJson[i].BankAddress1;
                    SelectedBanking.BankAddressFirst2 = SelectedBankingJson[i].BankAddress2;
                    SelectedBanking.IFSCCode1 = SelectedBankingJson[i].IFSCCode;
                    SelectedBanking.Sn = SelectedBankingJson[i + 1].Sn;
                    SelectedBanking.BankAccountNo2 = SelectedBankingJson[i + 1].BankAccountNo;
                    SelectedBanking.BankName2 = SelectedBankingJson[i + 1].BankName;
                    SelectedBanking.BankAddressSecond1 = SelectedBankingJson[i + 1].BankAddress1;
                    SelectedBanking.BankAddressSecond2 = SelectedBankingJson[i + 1].BankAddress2;
                    SelectedBanking.IFSCCode2 = SelectedBankingJson[i + 1].IFSCCode;
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task MapBussinessDetails()
        {
            try
            {
                storeAdditionalInfoCMI = _serviceProvider.CreateInstance<IStoreAdditionalInfoCMI>();
                brandInfos = new List<StoreBrandDealingIn>();
                storeAdditionalInfoCMI = EditOnBoardingDetails.StoreAdditionalInfoCMI;
                brandInfos = JsonConvert.DeserializeObject<List<StoreBrandDealingIn>>(EditOnBoardingDetails.StoreAdditionalInfoCMI.BrandDealingInDetails);
            }
            catch (Exception ex)
            {

            }
        }
        public async Task MapKartaDetails()
        {
            try
            {
                storeAdditionalInfoCMI = _serviceProvider.CreateInstance<IStoreAdditionalInfoCMI>();
                KartaEditDetails = new List<NameAndAddressOfProprietorModel>();
                storeAdditionalInfoCMI = EditOnBoardingDetails.StoreAdditionalInfoCMI;
                KartaEditDetails = JsonConvert.DeserializeObject<List<NameAndAddressOfProprietorModel>>(EditOnBoardingDetails.StoreAdditionalInfoCMI.PartnerDetails);
            }
            catch (Exception ex)
            {

            }
        }
        public async Task MapBussinessDetailsInCaseOfDistributor()
        {
            try
            {
                storeAdditionalInfoCMI = _serviceProvider.CreateInstance<IStoreAdditionalInfoCMI>();
                RACSalesByYearList = new List<StoreAdditionalInfoCMIRACSalesByYear1>();
                RetailingCityMonthlySalesList = new List<StoreAdditionalInfoCMIRetailingCityMonthlySales1>();
                storeAdditionalInfoCMI = EditOnBoardingDetails.StoreAdditionalInfoCMI;
                RACSalesByYearList = JsonConvert.DeserializeObject<List<StoreAdditionalInfoCMIRACSalesByYear1>>(EditOnBoardingDetails.StoreAdditionalInfoCMI.DistRacSalesByYear);
                RetailingCityMonthlySalesList = JsonConvert.DeserializeObject<List<StoreAdditionalInfoCMIRetailingCityMonthlySales1>>(EditOnBoardingDetails.StoreAdditionalInfoCMI.DistRetailingCityMonthlySales);
                for (int i = 0; i < 1; i++)
                {
                    RACSalesByYear.Year1 = RACSalesByYearList[i].Year;
                    RACSalesByYear.Year2 = RACSalesByYearList[i + 1].Year;
                    RACSalesByYear.Year3 = RACSalesByYearList[i + 2].Year;
                    RACSalesByYear.Qty1 = RACSalesByYearList[i].Qty;
                    RACSalesByYear.Qty2 = RACSalesByYearList[i + 1].Qty;
                    RACSalesByYear.Qty3 = RACSalesByYearList[i + 2].Qty;
                    RetailingCityMonthlySales.CitySD1 = RetailingCityMonthlySalesList[i].CityName;
                    RetailingCityMonthlySales.CitySD2 = RetailingCityMonthlySalesList[i + 1].CityName;
                    RetailingCityMonthlySales.CitySD3 = RetailingCityMonthlySalesList[i + 2].CityName;
                    RetailingCityMonthlySales.CitySD4 = RetailingCityMonthlySalesList[i + 3].CityName;
                    RetailingCityMonthlySales.CitySD5 = RetailingCityMonthlySalesList[i + 4].CityName;
                    RetailingCityMonthlySales.CityAMS1 = RetailingCityMonthlySalesList[i].AvgMonthlySales;
                    RetailingCityMonthlySales.CityAMS2 = RetailingCityMonthlySalesList[i + 1].AvgMonthlySales;
                    RetailingCityMonthlySales.CityAMS3 = RetailingCityMonthlySalesList[i + 2].AvgMonthlySales;
                    RetailingCityMonthlySales.CityAMS4 = RetailingCityMonthlySalesList[i + 3].AvgMonthlySales;
                    RetailingCityMonthlySales.CityAMS5 = RetailingCityMonthlySalesList[i + 4].AvgMonthlySales;
                    return;
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task MapEarlierWorkedWithCMIDetails()
        {
            try
            {
                StoreAdditionalInfoCMI = _serviceProvider.CreateInstance<IStoreAdditionalInfoCMI>();
                StoreAdditionalInfoCMI = EditOnBoardingDetails.StoreAdditionalInfoCMI;
            }
            catch (Exception ex)
            {

            }
        }
        private void AddCreateFields(Winit.Modules.Base.Model.IBaseModel baseModel, bool IsUIDRequired = true)
        {
            baseModel.CreatedBy = _appuser?.Emp?.UID ?? "ADMIN";
            baseModel.ModifiedBy = _appuser?.Emp?.UID ?? "ADMIN";
            baseModel.CreatedTime = DateTime.Now;
            baseModel.ModifiedTime = DateTime.Now;
            if (IsUIDRequired) baseModel.UID = Guid.NewGuid().ToString();
        }
        public async Task<bool> HandleSelfRegistration()
        {
            try
            {
                AddCreateFields(selfRegistration);
                return await SaveAndUpdateSelfRegistration(selfRegistration);
            }
            catch (Exception ex)
            { throw; }
        }
        public async Task<bool> VerifyOTP()
        {
            try
            {
                return await HandleVerifyOTP(selfRegistration);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<bool> CreateUpdateDocumentFileSysDataAppendix(List<List<Winit.Modules.FileSys.Model.Interfaces.IFileSys>> fileSys)
        {
            DocumentFilesys = fileSys;
            if (DocumentFilesys != null)
            {
                await CreateUpdateDocumentFileSysDataAppendixDetails(DocumentFilesys);
                DocumentAppendixfileSysList = fileSys;
                return true;
            }
            return false;
        }
        public async Task<Dictionary<string, int>> GetTabItemsCount(List<FilterCriteria> filterCriterias)
        {
            try
            {
                return await GetTabItemsCountFromApi(filterCriterias);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public abstract Task<Dictionary<string, int>> GetTabItemsCountFromApi(List<FilterCriteria> filterCriterias);
        public abstract Task<bool> SaveAndUpdateSelfRegistration(Winit.Modules.Store.Model.Interfaces.ISelfRegistration selfRegistration);
        public abstract Task<bool> HandleVerifyOTP(Winit.Modules.Store.Model.Interfaces.ISelfRegistration selfRegistration);

        public abstract Task<List<Winit.Modules.Store.Model.Classes.Store>> GetBroadClassificationDetails();
        public abstract Task<List<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetAllASM(string BranchUID);
        public abstract Task<List<IBroadClassificationLine>> GetBroadClassificationLineDetails(string UID);
        public abstract Task<List<ILocation>> GetStateDetails(List<string> locationTypes);
        public abstract Task<List<IOrg>> GetOUDetails(string OrgType);
        public abstract Task<List<IOrg>> GetDivisionDetails(string OrgType, string ParentUID);
        public abstract Task<ILocation> GetCountryAndRegionDetails(string UID, string Type);
        public abstract Task<List<ILocation>> GetCityAndLocalityDetails(List<string> UID);
        public abstract Task<List<IStoreCredit>> GetStoreCredit();
        public abstract Task<List<IEmp>> GetASMDetailsFromBranch(string BranchUID, string Code);
        public abstract Task<List<IEmp>> GetASEMDetailsFromBranch(string BranchUID, string Code);
        public abstract Task<List<ISalesOffice>> GetSalesOfficeDetailsFromBranch(string BranchUID);
        public abstract Task<List<IBranch>> GetBranchDetails(BranchHeirarchy UID);
        public abstract Task<List<IAllApprovalRequest>> GetAllApproveListDetails(string UID);
        public abstract Task<IOnBoardEditCustomerDTO> GetAllOnBoardingDetailsByStoreUID(string UID);
        public abstract Task<bool> CreateUpdatecontact(IContact contact, bool IsCreate);
        public abstract Task<bool> CreateUpdateCustomerInformation(IOnBoardCustomerDTO onBoardCustomerDTO, bool IsCreate);
        public abstract Task<bool> CreateUpdateaddress(IAddress address, bool IsCreate);
        public abstract Task<bool> CreateGstAddress(List<IAddress> address);
        public abstract Task<bool> CreateUpdateAsmDivisionMapping(List<IAsmDivisionMapping> asmDivisionMapping, bool IsCreate);
        public abstract Task<bool> CreateUpdateEmployeeDetails(IStoreAdditionalInfoCMI storeAdditionalInfoCMI);
        public abstract Task<bool> SaveApprovalRequestDetails(string RequestId);
        public abstract Task GetRuleIdData(string Type, string TypeCode);
        public abstract Task GetAllUserHierarchyData(string hierarchyType, string hierarchyUID, int ruleId);
        public abstract Task<bool> CreateUpdateDistDetails(StoreApprovalDTO storeApprovalDTO);
        public abstract Task<List<IContact>> GetAllContactData();
        public abstract Task<List<IAddress>> GetAllAddressData(string Type);
        public abstract Task<Winit.Modules.Store.Model.Classes.Store> CheckExistOrNot(string UID);
        public abstract Task<string> DeleteContactDetailsFromGrid(string uid);
        public abstract Task<bool> InsertDataInChangeRequestTable(string changeRecordDTOJson);
        public abstract Task<string> DeleteAddressDetailsFromGrid(string uid);
        public abstract Task<List<CommonUIDResponse>> CreateUpdateDocumentFileSysData(List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> fileSys, bool IsCreate);
        public abstract Task<bool> CreateUpdateDocumentFileSysDataAppendixDetails(List<List<Winit.Modules.FileSys.Model.Interfaces.IFileSys>> fileSys);
        public abstract Task<List<IOnBoardGridview>> GetOnBoardDetailsGridiview();
        public abstract Task<List<IAllApprovalRequest>> GetApprovalStatus(string LinkItemUID);
        public abstract Task<int> DeleteOnBoardingDetails(string UID);
        public abstract Task<int> DeleteAsmDivisionMapping(string UID);
        public abstract Task<List<IAsmDivisionMapping>> GetAsmDetailsByUID(string LinkedItemType, string LinkedItemUID);
        public abstract Task<List<IOrg>> GetAllDivisionDetails();
        public abstract Task<bool> CreateDistributorAfterFinalApproval(IStore store);
        public abstract Task<IEmp> GetBMByBranchUID(string UID);


        #region  Calling GstNumValidationAPI
        public async Task<bool> IsGstUnique(string gstNumber)
        {
            try
            {
                var gstDetails = await _apiService.FetchDataAsync(
               $"{_appConfigs.ApiBaseUrl}Store/IsGstUnique?GStNumber={gstNumber}", HttpMethod.Get);
                if (gstDetails.IsSuccess)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(gstDetails.Data);
                    return Convert.ToBoolean(data);
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> GetGstDetails(string gstNumber)
        {
            try
            {
                GSTINDetails = new GSTINDetailsModel();
                var gstNumDetails = await _apiService.FetchDataAsync(
               $"{_appConfigs.ApiBaseUrl}GST/GetGstNumDetail", HttpMethod.Post, gstNumber);
                if (gstNumDetails.IsSuccess && gstNumDetails.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(gstNumDetails.Data);
                    GSTINDetails = ConvertSearchGSTINJsonToModel(gstNumDetails.Data);
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
        private GSTINDetailsModel ConvertSearchGSTINJsonToModel(string jsonString)
        {
            var jsonData = JObject.Parse(jsonString);
            var data = jsonData["Data"];

            var NumberOfPrincipalAddress = data?["adadr"]?.HasValues == true && data["adadr"]?.Count() > 0 ? data?["adadr"]!.Count() : 0;
            for (int i = 0; i < NumberOfPrincipalAddress; i++)
            {
                GSTINDetailsModel gSTINDetailsModel = new GSTINDetailsModel()
                {
                    AR_ADR_BuildingName = data!["adadr"]?.HasValues == true && data["adadr"]?.Count() > 0 ? data["adadr"]?[i]?["addr"]?["bnm"]?.ToString() : null,
                    AR_ADR_Location = data["adadr"]?.HasValues == true && data["adadr"]?.Count() > 0 ? data["adadr"]?[i]?["addr"]?["loc"]?.ToString() : null,
                    AR_ADR_Street = data["adadr"]?.HasValues == true && data["adadr"]?.Count() > 0 ? data["adadr"]?[i]?["addr"]?["st"]?.ToString() : null,
                    AR_ADR_DoorNo = data["adadr"]?.HasValues == true && data["adadr"]?.Count() > 0 ? data["adadr"]?[i]?["addr"]?["bno"]?.ToString() : null,
                    AR_ADR_District = data["adadr"]?.HasValues == true && data["adadr"]?.Count() > 0 ? data["adadr"]?[i]?["addr"]?["dst"]?.ToString() : null,
                    AR_ADR_Latitude = data["adadr"]?.HasValues == true && data["adadr"]?.Count() > 0 && !string.IsNullOrEmpty(data["adadr"]?[i]?["addr"]?["lt"]?.ToString())
                    ? data["adadr"]?[i]?["addr"]?["lt"]?.ToObject<double>() ?? 0
                    : 0,
                    AR_ADR_Locality = data["adadr"]?.HasValues == true && data["adadr"]?.Count() > 0 ? data["adadr"]?[i]?["addr"]?["locality"]?.ToString() : null,
                    AR_ADR_Pincode = data["adadr"]?.HasValues == true && data["adadr"]?.Count() > 0 ? data["adadr"]?[i]?["addr"]?["pncd"]?.ToString() : null,
                    AR_ADR_Landmark = data["adadr"]?.HasValues == true && data["adadr"]?.Count() > 0 ? data["adadr"]?[i]?["addr"]?["landMark"]?.ToString() : null,
                    AR_ADR_State = data["adadr"]?.HasValues == true && data["adadr"]?.Count() > 0 ? data["adadr"]?[i]?["addr"]?["stcd"]?.ToString() : null,
                    AR_ADR_GeoCodeLevel = data["adadr"]?.HasValues == true && data["adadr"]?.Count() > 0 ? data["adadr"]?[i]?["addr"]?["geocodelvl"]?.ToString() : null,
                    AR_ADR_FloorNo = data["adadr"]?.HasValues == true && data["adadr"]?.Count() > 0 ? data["adadr"]?[i]?["addr"]?["flno"]?.ToString() : null,
                    AR_ADR_Longitude = data["adadr"]?.HasValues == true && data["adadr"]?.Count() > 0 && !string.IsNullOrEmpty(data["adadr"]?[i]?["addr"]?["lg"]?.ToString())
                    ? data["adadr"]?[i]?["addr"]?["lg"]?.ToObject<double>() ?? 0
                    : 0,
                    AR_NatureOfBusiness = data["adadr"]?.HasValues == true && data["adadr"]?.Count() > 0 ? data["adadr"]?[i]?["ntr"]?.ToString() : null,
                    IsPrimary = false
                };
                var addressParts = new List<string?>
                                {
                                    gSTINDetailsModel.AR_ADR_DoorNo,
                                    gSTINDetailsModel.AR_ADR_FloorNo,
                                    gSTINDetailsModel.AR_ADR_BuildingName,
                                    gSTINDetailsModel.AR_ADR_Street,
                                    gSTINDetailsModel.AR_ADR_Location
                                };
                string shipToAddress = string.Join(", ", addressParts.Where(part => !string.IsNullOrEmpty(part)));
                gSTINDetailsModel.Address = shipToAddress;
                ShipToAddressFromGst.Add(gSTINDetailsModel);
            }
            BillToAddressFromGst.AR_ADR_BuildingName = data!["pradr"]?["addr"]?["bnm"]?.ToString();
            BillToAddressFromGst.PR_ADR_Location = data["pradr"]?["addr"]?["loc"]?.ToString();
            BillToAddressFromGst.PR_ADR_Street = data["pradr"]?["addr"]?["st"]?.ToString();
            BillToAddressFromGst.PR_ADR_DoorNo = data["pradr"]?["addr"]?["bno"]?.ToString();
            BillToAddressFromGst.PR_ADR_District = data["pradr"]?["addr"]?["dst"]?.ToString();
            BillToAddressFromGst.PR_ADR_Latitude = !string.IsNullOrEmpty(data["pradr"]?["addr"]?["lt"]?.ToString())
              ? data["pradr"]?["addr"]?["lt"]?.ToObject<double>() ?? 0
              : 0;
            BillToAddressFromGst.PR_ADR_Locality = data["pradr"]?["addr"]?["locality"]?.ToString();
            BillToAddressFromGst.PR_ADR_Pincode = data["pradr"]?["addr"]?["pncd"]?.ToString();
            BillToAddressFromGst.PR_ADR_Landmark = data["pradr"]?["addr"]?["landMark"]?.ToString();
            BillToAddressFromGst.PR_ADR_State = data["pradr"]?["addr"]?["stcd"]?.ToString();
            BillToAddressFromGst.PR_ADR_GeoCodeLevel = data["pradr"]?["addr"]?["geocodelvl"]?.ToString();
            BillToAddressFromGst.PR_ADR_FloorNo = data["pradr"]?["addr"]?["flno"]?.ToString();
            BillToAddressFromGst.PR_ADR_Longitude = !string.IsNullOrEmpty(data["pradr"]?["addr"]?["lg"]?.ToString())
               ? data["pradr"]?["addr"]?["lg"]?.ToObject<double>() ?? 0
               : 0;
            BillToAddressFromGst.PR_NatureOfBusiness = data["pradr"]?["ntr"]?.ToString();
            return new GSTINDetailsModel
            {
                StateJurisdictionCode = data!["stjCd"]?.ToString(),
                Duty = data["dty"]?.ToString(),
                LegalName = data["lgnm"]?.ToString(),
                StateJurisdiction = data["stj"]?.ToString(),
                CancellationDate = data["cxdt"]?.ToString(),
                GSTIN = data["gstin"]?.ToString(),
                NatureOfBusinessActivity = data["nba"] != null ? string.Join(", ", data["nba"]) : string.Empty,
                LastUpdate = data["lstupdt"]?.ToString(),
                ConstitutionOfBusiness = data["ctb"]?.ToString(),
                RegistrationDate = ConvertToDateFormat(data["rgdt"]?.ToString()),
                PR_ADDR_BuildingName = data["pradr"]?["addr"]?["bnm"]?.ToString(),
                PR_ADR_Location = data["pradr"]?["addr"]?["loc"]?.ToString(),
                PR_ADR_Street = data["pradr"]?["addr"]?["st"]?.ToString(),
                PR_ADR_DoorNo = data["pradr"]?["addr"]?["bno"]?.ToString(),
                PR_ADR_District = data["pradr"]?["addr"]?["dst"]?.ToString(),
                PR_ADR_Latitude = !string.IsNullOrEmpty(data["pradr"]?["addr"]?["lt"]?.ToString())
                  ? data["pradr"]?["addr"]?["lt"]?.ToObject<double>() ?? 0
                  : 0,
                PR_ADR_Locality = data["pradr"]?["addr"]?["locality"]?.ToString(),
                PR_ADR_Pincode = data["pradr"]?["addr"]?["pncd"]?.ToString(),
                PR_ADR_Landmark = data["pradr"]?["addr"]?["landMark"]?.ToString(),
                PR_ADR_State = data["pradr"]?["addr"]?["stcd"]?.ToString(),
                PR_ADR_GeoCodeLevel = data["pradr"]?["addr"]?["geocodelvl"]?.ToString(),
                PR_ADR_FloorNo = data["pradr"]?["addr"]?["flno"]?.ToString(),
                PR_ADR_Longitude = !string.IsNullOrEmpty(data["pradr"]?["addr"]?["lg"]?.ToString())
                   ? data["pradr"]?["addr"]?["lg"]?.ToObject<double>() ?? 0
                   : 0,
                PR_NatureOfBusiness = data["pradr"]?["ntr"]?.ToString(),
                Status = data["sts"]?.ToString(),
                CentralJurisdictionCode = data["ctjCd"]?.ToString(),
                TradeName = data["tradeNam"]?.ToString(),
                CentralJurisdiction = data["ctj"]?.ToString(),
                EInvoiceStatus = data["einvoiceStatus"]?.ToString(),

            };
        }

        public static string ConvertToDateFormat(string dateString)
        {
            // Parse the date string in the "dd/MM/yyyy" format
            DateTime date = DateTime.ParseExact(dateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            // Return the date in "yyyy-MM-dd" format
            return date.ToString("yyyy-MM-dd");
        }



        #endregion

        public class BranchHeirarchy
        {
            public int SlNo { get; set; }
            public string UID { get; set; }
            public string Name { get; set; }
        }
    }

}
