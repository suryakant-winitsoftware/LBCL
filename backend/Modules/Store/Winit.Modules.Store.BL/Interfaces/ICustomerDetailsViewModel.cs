using Winit.Modules.Address.Model.Interfaces;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.BroadClassification.Model.Interfaces;
using Winit.Modules.Contact.Model.Interfaces;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.FileSys.Model.Classes;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common.GST;

namespace Winit.Modules.Store.BL.Interfaces
{
    public interface ICustomerDetailsViewModel
    {
        public Dictionary<string, List<string>> UserRole_Code { get; set; }

        public string NewlyGeneratedUID { get; set; }
        public bool IsLogin { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public List<IContact> ContactList { get; set; }
        public List<IAddress> AddressList { get; set; }
        public string CustomerCode { get; set; }
        public string StoreUID { get; set; }
        public string UserCode { get; set; }
        public string SelfRegistrationUID { get; set; }
        public string UserType { get; set; }
        public string UserRoleCode { get; set; }
        public int RuleId { get; set; }
        public string CustomerCreationStoreUID { get; set; }
        public string TabName { get; set; }
        public IContact Contact { get; set; }
        public IAddress Address { get; set; }
        public string BroadClassfication { get; set; }
        public Winit.Modules.Store.Model.Interfaces.ISelfRegistration selfRegistration { get; set; }
        public GSTINDetailsModel GSTINDetails { get; set; }
        void GenerateCustomerCode();
        Task<Winit.Modules.Store.Model.Classes.Store> CheckStoreExistsOrNot(string UID);
        Task<List<IContact>> GetAllContacts();
        Task<List<IAddress>> GetAllAddress(string Type);
        Task<bool> SaveUpdateContact(IContact contact, bool Iscreate);
        Task<bool> SaveUpdateCustomerInformation(IOnBoardCustomerDTO onBoardCustomerDTO, bool Iscreate);
        Task<bool> SaveUpdateBillToAddress(IAddress address, bool Iscreate);
        Task<bool> SaveUpdateShipToAddress(IAddress address, bool Iscreate);
        Task<bool> SaveUpdateAsmDivision(List<IAsmDivisionMapping> asmDivisionMapping, bool Iscreate);
        Task<bool> SaveUpdateEmployeeDetails(IStoreAdditionalInfoCMI storeAdditionalInfoCMI);
        Task<bool> SaveUpdateDistDetails(StoreApprovalDTO storeApprovalDTO);
        Task<bool> InsertDataInChangeRequest(string changeRecordDTOJson);
        //Task<bool> SaveUpdateShowroomDetails(IStoreAdditionalInfoCMI storeAdditionalInfoCMI, bool Iscreate);
        Task<string> DeleteContactDetails(string UID);
        Task<string> DeleteAddressDetails(string UID);
        Task<bool> GetGstDetails(string gstNumber);
        Task<bool> IsGstUnique(string gstNumber);
        Task<List<Winit.Modules.Store.Model.Classes.Store>> GetBroadClassificationDetails();
        Task<List<IBroadClassificationLine>> GetBroadClassificationLineDetails(string UID);
        Task<List<IAllApprovalRequest>> GetAllApproveListDetails(string UID);
        public string StoreAdditionalInfoCMIUid { get; set; }
        Task<List<CommonUIDResponse>> CreateUpdateDocumentFileSysData(List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> fileSys, bool IsCreate);
        Task<bool> CreateUpdateDocumentFileSysDataAppendix(List<List<Winit.Modules.FileSys.Model.Interfaces.IFileSys>> fileSys);
        Task<List<IAllApprovalRequest>> GetApprovalStatusDetails(string LinkItemUID);
        Task PageIndexChanged(int pageNumber);
        Task OnSorting(SortCriteria sortCriteria);
        Task GetStates();
        Task GetCountryAndRegionByState(string UID, string Type);
        Task<bool> SaveAllApprovalRequestDetails(string RequestId);

        //Gridview
        public List<IOnBoardGridview> onBoardGridviewList { get; set; }
        public List<IAllApprovalRequest> AllApprovalLevelList { get; set; }
        Task PopulateViewModel();
        //customer information
        public IOnBoardCustomerDTO? _onBoardCustomerDTO { get; set; }
        public List<List<IFileSys>>? DocumentAppendixfileSysList { get; set; }
        public IOnBoardCustomerDTO? _originalOnBoardCustomerDTO { get; set; }
        public List<ISelectionItem> CustomerClassificationselectionItems { get; set; }
        public List<ISelectionItem> ClassificationTypeselectionItems { get; set; }
        public List<ISelectionItem> FirmTypeselectionItems { get; set; }
        public List<ISelectionItem> WayOfCommunication { get; set; }
        public List<ISelectionItem> AooaTypeselectionItems { get; set; }
        //contact details
        public IContact SelectedContact { get; set; }
        public List<IContact> Contacts { get; set; }
        public IAddress SelectedBillToAddress { get; set; }
        public IAddress SelectedShipToAddress { get; set; }
        public List<IAddress> ShipToAddresses { get; set; }
        public List<IAddress> OriginalShipToAddresses { get; set; }
        public List<IAddress> BillToAddresses { get; set; }
        public List<IAddress> OriginalBillToAddresses { get; set; }
        public IStoreBanking SelectedBanking { get; set; }
        public IStoreBanking OriginalSelectedBanking { get; set; }
        public IStoreAdditionalInfoCMI storeAdditionalInfoCMI { get; set; }
        public IStoreAdditionalInfoCMIRetailingCityMonthlySales RetailingCityMonthlySales { get; set; }
        public IStoreAdditionalInfoCMIRetailingCityMonthlySales OriginalRetailingCityMonthlySales { get; set; }
        public IStoreAdditionalInfoCMIRACSalesByYear RACSalesByYear { get; set; }
        public IStoreAdditionalInfoCMIRACSalesByYear OriginalRACSalesByYear { get; set; }

        public List<StoreBrandDealingIn> brandInfos { get; set; }
        public List<NameAndAddressOfProprietorModel> KartaEditDetails { get; set; }
        public List<StoreBrandDealingIn> OriginalBrandInfos { get; set; }
        public List<SupervisorInfo> SupervisorInfos { get; set; }
        public List<TechniciansInfo> TechnicianInfo { get; set; }
        public List<SupervisorInfo> OriginalSupervisorInfos { get; set; }
        public List<TechniciansInfo> OriginalTechnicianInfo { get; set; }
        public IStoreAdditionalInfoCMI StoreAdditionalInfoCMI { get; set; }
        public List<IStoreShowroom> ShowroomDetails { get; set; }
        public List<IStoreShowroom> OriginalShowroomDetails { get; set; }
        public IStoreSignatory storeSignatory { get; set; }
        public IStoreSignatory OriginalstoreSignatory { get; set; }
        public List<IAllApprovalRequest> ApprovalStatus { get; set; }
        public List<IStoreSignatory> SignatoryDetails { get; set; }
        public List<IStoreSignatory> OriginalSignatoryDetails { get; set; }
        public List<ISelectionItem> StateselectionItems { get; set; }
        public List<ISelectionItem> CityselectionItems { get; set; }
        public List<ISelectionItem> LocalityselectionItems { get; set; }
        public List<ISelectionItem> BranchselectionItems { get; set; }
        public List<ISelectionItem> AllASMselectionItems { get; set; }
        public List<ISelectionItem> ASMselectionItems { get; set; }
        public List<ISelectionItem> DivisionMappingselectionItems { get; set; }
        public List<ISelectionItem> AsmMappingselectionItems { get; set; }
        public List<ISelectionItem> ASEMMappingselectionItems { get; set; }
        public List<ISelectionItem> OUselectionItems { get; set; }
        public List<ISelectionItem> DivisionSelectionItems { get; set; }
        public List<ISelectionItem> OUselectionItemsForShipTo { get; set; }
        public List<ISelectionItem> SalesOfficeselectionItems { get; set; }
        public List<ISelectionItem> PinCodeselectionItems { get; set; }
        public List<ISelectionItem> StateselectionItemsForShipTo { get; set; }
        public List<ISelectionItem> CityselectionItemsForShipTo { get; set; }
        public List<ISelectionItem> LocalityselectionItemsForShipTo { get; set; }
        public List<ISelectionItem> BranchselectionItemsForShipTo { get; set; }
        public List<ISelectionItem> SalesOfficeselectionItemsForShipTo { get; set; }
        public List<ISelectionItem> PinCodeselectionItemsForShipTo { get; set; }
        public List<ISelectionItem> ASMselectionItemsForShipTo { get; set; }
        public List<ISelectionItem> ASEMselectionItemsForShipTo { get; set; }
        public List<IStoreBankingJson> SelectedBankingJson { get; set; }
        public List<IAsmDivisionMapping> AsmDivisionMappingDetails { get; set; }
        public List<IAsmDivisionMapping> AsmMappingGridDetails { get; set; }
        public List<ISelectionItem> StateselectionItemsForShowRoom { get; set; }
        public List<ISelectionItem> CityselectionItemsForShowRoom { get; set; }
        public List<ISelectionItem> LocalityselectionItemsForShowRoom { get; set; }
        public List<ISelectionItem> PinCodeselectionItemsForShowRoom { get; set; }
        public List<ISelectionItem> BranchselectionItemsForShowRoom { get; set; }
        public List<ISelectionItem> DivisionDetails { get; set; }
        public List<StoreAdditionalInfoCMIRetailingCityMonthlySales1> RetailingCityMonthlySalesList { get; set; }
        public List<StoreAdditionalInfoCMIRetailingCityMonthlySales1> OriginalRetailingCityMonthlySalesList { get; set; }

        public List<StoreAdditionalInfoCMIRACSalesByYear1> RACSalesByYearList { get; set; }
        public List<StoreAdditionalInfoCMIRACSalesByYear1> OriginalRACSalesByYearList { get; set; }

        public List<ISelectionItem> ContactTypeSelectionItems { get; set; }
        //Address details
        public bool IsRejectButtonNeeded { get; set; }
        public bool CustomerEditApprovalRequired { get; set; }
        public bool IsCodeOfEthics { get; set; }
        Task GetRuleId(string Type, string TypeCode);
        public List<ISelectionItem> GenderselectionItems { get; set; }
        public IOnBoardEditCustomerDTO EditOnBoardingDetails { get; set; }
        public Winit.Modules.Contact.Model.Classes.Contact? _originalContact { get; set; }
        public List<List<IFileSys>> DocumentFilesys { get; set; }
        public List<GSTINDetailsModel> ShipToAddressFromGst { get; set; }
        public GSTINDetailsModel BillToAddressFromGst { get; set; }
        Task OnChangeBroadClassification(string UID);
        Task MapOnBoardDetails();
        Task CreateInstances();
        Task PopulateOnBoardDetails(string CustomerID);
        public abstract Task<int> DeleteOnBoardingDetails(string UID);

        // self-registration
        Task<bool> HandleSelfRegistration();
        Task<bool> VerifyOTP();
        Task OnStateSelection(List<string> UID);
        Task AsmOnload();
        Task OnCitySelection(List<string> UID);
        Task OnLocalitySelection(List<string> UID);
        Task OnBranchSelection(string BranchUID);
        Task OnStateSelectionForShipTo(List<string> UID);
        Task OnCitySelectionForShipTo(List<string> UID);
        Task OnLocalitySelectionForShipTo(List<string> UID);
        Task OnStateSelectionForShowRoom(List<string> UID);
        Task OnCitySelectionForShowRoom(List<string> UID);
        Task OnLocalitySelectionForShowRoom(List<string> UID);
        Task OnBranchSelectionForShipTo(string BranchUID);
        Task OnOUSelectionForShipTo(string ParentUID);
        //Task OnEditSelectionForBillTo(string UID);
        Task OnEditSelectionForShipTo(IAddress UID);
        Task OnEditSelectionForShowRoom(IStoreShowroom UID);
        Task OnCopySelectionForShipTo(IAddress address);
        Task OnEditSelectionForBillTo(IAddress address);
        Task OnCopyGSTBillTo(GSTINDetailsModel address);
        Task GetAllUserHierarchy(string hierarchyType, string hierarchyUID, int ruleId);
        Task GetAsmDivisionDetails(string UID);
        Task GetDivisionDetails();
        Task<int> DeleteAsmDivisionMapping(string UID);
        Task PopulateGstAddress(List<IAddress> address);
        Task<bool> CreateDistributorAfterFinalApproval(IStore store);
        Task OnFilterApply(Dictionary<string, string> keyValuePairs);
        Task<IEmp> GetBMByBranchUID(string UID);

        Task<List<ISelectionItem>> GetAllAsmByBranchUID(string BranchUID);

        Task<List<IOnBoardGridview>> GetOnBoardDetailsGridiview();
        public string CurrentStatus { get; set; }
        public List<FilterCriteria> FilterCriterias { get; set; }
        Task<Dictionary<string, int>> GetTabItemsCount(List<FilterCriteria> filterCriterias);
        Task<List<ISelectionItem>> GetAllAsemByBranchUID(string BranchUID);

    }
}
