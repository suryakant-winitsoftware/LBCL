using Newtonsoft.Json;
using Winit.Modules.Address.Model.Interfaces;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.Model;
using Winit.Modules.BroadClassification.Model.Classes;
using Winit.Modules.BroadClassification.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Contact.Model.Interfaces;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.FileSys.Model.Classes;
using Winit.Modules.Int_CommonMethods.Model.Classes;
using Winit.Modules.Location.Model.Constants;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Constants;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.User.Model.Classes;
using Winit.Modules.User.Model.Constants;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.SnackBar;
using Winit.UIModels.Common;

namespace Winit.Modules.Store.BL.Classes
{
    public class CustomerDetailsWebViewModel : CustomerDetailsBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private Winit.Modules.Common.BL.Interfaces.IAppUser _appuser;
        private IToast _toast;
        public Winit.Modules.Setting.BL.Interfaces.IAppSetting _appSetting;
        public CustomerDetailsWebViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IAppUser appuser,
            IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService, Setting.BL.Interfaces.IAppSetting appSetting)
        : base(serviceProvider, filter, sorter, appuser, listHelper, appConfigs, apiService, appSetting)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _appuser = appuser;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            //_toast = toast;
            _appSetting = appSetting;

        }
        private void AddCreateFields(IBaseModel baseModel, bool IsUIDRequired)
        {

            baseModel.CreatedBy = _appuser?.Emp?.UID ?? "ADMIN";
            baseModel.ModifiedBy = _appuser?.Emp?.UID ?? "ADMIN";
            baseModel.CreatedTime = DateTime.Now;
            baseModel.ModifiedTime = DateTime.Now;
            if (IsUIDRequired) baseModel.UID = Guid.NewGuid().ToString();
        }
        private void AddUpdateFields(IBaseModel baseModel)
        {
            baseModel.ModifiedBy = _appuser?.Emp?.UID ?? "ADMIN";
            baseModel.ModifiedTime = DateTime.Now;
        }
        public override async Task<List<IOnBoardGridview>> GetOnBoardDetailsGridiview()
        {
            return await GetOnBoardDetailsGridiviewFromAPIAsync();
        }
        public override async Task<List<IAllApprovalRequest>> GetApprovalStatus(string LinkItemUID)
        {
            return await GetApprovalStatusFromAPIAsync(LinkItemUID);
        }
        public override async Task<int> DeleteOnBoardingDetails(string UID)
        {
            return await DeleteOnBoardingDetailsGridiviewFromAPIAsync(UID);
        }
        public override async Task<int> DeleteAsmDivisionMapping(string UID)
        {
            return await DeleteAsmDivisionMappingFromAPIAsync(UID);
        }
        public override Task<bool> CreateUpdatecontact(IContact contact, bool IsCreate)
        {
            return CreateUpdatecontactFromAPIAsync(contact, IsCreate);
        }
        public override Task<bool> CreateUpdateCustomerInformation(IOnBoardCustomerDTO onBoardCustomerDTO, bool IsCreate)
        {
            return CreateUpdateCustomerInformationFromAPIAsync(onBoardCustomerDTO, IsCreate);
        }
        public override async Task<List<IContact>> GetAllContactData()
        {
            return await GetAllContactDataFromAPIAsync();
        }

        public override async Task<string> DeleteContactDetailsFromGrid(string uid)
        {
            return await DeleteContactDetailsFromGridFromAPIAsync(uid);
        }

        public override async Task<bool> InsertDataInChangeRequestTable(string changeRecordDTOJson)
        {
            return await InsertDataInChangeRequestTableAsync(changeRecordDTOJson);
        }
        public override Task<bool> CreateUpdateaddress(IAddress address, bool IsCreate)
        {
            return CreateUpdateaddressFromAPIAsync(address, IsCreate);
        }
        public override Task<bool> CreateGstAddress(List<IAddress> address)
        {
            return CreateGstAddressFromAPIAsync(address);
        }
        public override Task<bool> CreateUpdateAsmDivisionMapping(List<IAsmDivisionMapping> asmDivisionMapping, bool IsCreate)
        {
            return CreateUpdateAsmDivisionMappingFromAPIAsync(asmDivisionMapping, IsCreate);
        }
        public override Task<bool> CreateUpdateEmployeeDetails(IStoreAdditionalInfoCMI storeAdditionalInfoCMI)
        {
            return CreateUpdateEmployeeDetailsFromAPIAsync(storeAdditionalInfoCMI);
        }
        public override Task GetRuleIdData(string Type, string TypeCode)
        {
            return GetRuleIdFromAPIAsync(Type, TypeCode);
        }
        public override Task GetAllUserHierarchyData(string hierarchyType, string hierarchyUID, int ruleId)
        {
            return GetAllUserHierarchyFromAPIAsync(hierarchyType, hierarchyUID, ruleId);
        }
        public override Task<bool> SaveApprovalRequestDetails(string RequestId)
        {
            return SaveApprovalRequestDetailsFromAPIAsync(RequestId);
        }
        public override Task<bool> CreateUpdateDistDetails(StoreApprovalDTO storeApprovalDTO)
        {
            return CreateUpdateDistDetailsFromAPIAsync(storeApprovalDTO);
        }
        public override async Task<List<IAddress>> GetAllAddressData(string Type)
        {
            return await GetAllAddressDataFromAPIAsync(Type);
        }
        public override async Task<Winit.Modules.Store.Model.Classes.Store> CheckExistOrNot(string UID)
        {
            return await CheckExistOrNotFromAPIAsync(UID);
        }
        public override async Task<string> DeleteAddressDetailsFromGrid(string uid)
        {
            return await DeleteAddressDetailsFromGridFromAPIAsync(uid);
        }
        public override async Task<List<Winit.Modules.Store.Model.Classes.Store>> GetBroadClassificationDetails()
        {
            return await GetBroadClassificationDetailsFromAPIAsync();
        }
        public override async Task<List<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetAllASM(string BranchUID)
        {
            return await GetAllASMDetailsFromAPIAsync(BranchUID);
        }
        public override async Task<List<IBroadClassificationLine>> GetBroadClassificationLineDetails(string UID)
        {
            return await GetBroadClassificationLineDetailsFromAPIAsync(UID);
        }
        public override async Task<ILocation> GetCountryAndRegionDetails(string UID, string Type)
        {
            return await GetCountryAndRegionDetailsFromAPI(UID, Type);
        }
        public override async Task<List<ILocation>> GetStateDetails(List<string> locationTypes)
        {
            return await GetStateDetailsFromAPIAsync(locationTypes);
        }
        public override async Task<List<IOrg>> GetOUDetails(string OrgType)
        {
            return await GetOUDetailsFromAPIAsync(OrgType);
        }
        public override async Task<List<IOrg>> GetDivisionDetails(string OrgType, string ParentUID)
        {
            return await GetDivisionDetailsFromAPIAsync(OrgType, ParentUID);
        }
        public override async Task<List<IOrg>> GetAllDivisionDetails()
        {
            return await GetAllDivisionDetailsFromAPIAsync();
        }
        public override async Task<List<ILocation>> GetCityAndLocalityDetails(List<string> locationTypes)
        {
            return await GetCityAndLocalityDetailsFromAPIAsync(locationTypes);
        }
        public override async Task<List<IStoreCredit>> GetStoreCredit()
        {
            return await GetStoreCreditDetailsFromAPIAsync();
        }
        public override async Task<List<IBranch>> GetBranchDetails(BranchHeirarchy locationTypes)
        {
            return await GetBranchDetailsFromAPIAsync(locationTypes);
        }
        public override async Task<List<IEmp>> GetASMDetailsFromBranch(string UID, string Code)
        {
            return await GetASMDetailsFromBranchFromAPIAsync(UID, Code);
        }
        public override async Task<List<IEmp>> GetASEMDetailsFromBranch(string UID, string Code)
        {
            return await GetASEMDetailsFromBranchFromAPIAsync(UID, Code);
        }
        public override async Task<List<ISalesOffice>> GetSalesOfficeDetailsFromBranch(string UID)
        {
            return await GetSalesOfficeDetailsFromBranchFromAPIAsync(UID);
        }
        public override async Task<List<IAllApprovalRequest>> GetAllApproveListDetails(string UID)
        {
            return await GetAllApproveListDetailsFromAPIAsync(UID);
        }
        public override async Task<IOnBoardEditCustomerDTO> GetAllOnBoardingDetailsByStoreUID(string UID)
        {
            return await GetAllOnBoardingDetailsByStoreUIDFromAPIAsync(UID);
        }
        public override async Task<List<CommonUIDResponse>> CreateUpdateDocumentFileSysData(List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> fileSys, bool IsCreate)
        {
            return await SaveDocumentFileSysDataFromAPIAsync(fileSys, IsCreate);
        }
        public override async Task<bool> CreateUpdateDocumentFileSysDataAppendixDetails(List<List<Winit.Modules.FileSys.Model.Interfaces.IFileSys>> fileSys)
        {
            return await CreateUpdateDocumentFileSysDataAppendixFromAPIAsync(fileSys);
        }
        public override async Task<List<IAsmDivisionMapping>> GetAsmDetailsByUID(string LinkedItemType, string LinkedItemUID)
        {
            return await GetAsmDetailsByUIDFromAPIAsync(LinkedItemType, LinkedItemUID);
        }
        public async Task<List<Winit.Modules.Store.Model.Interfaces.IOnBoardGridview>> GetOnBoardDetailsGridiviewFromAPIAsync()
        {
            try
            {

                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias ??= new List<FilterCriteria>();
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.PageSize = PageSize;
                pagingRequest.FilterCriterias = FilterCriterias;
                // pagingRequest.FilterCriterias.Add(new FilterCriteria("status", CurrentStatus, FilterType.Equal)); add more filter criteria
                pagingRequest.IsCountRequired = true;
                pagingRequest.SortCriterias = (SortCriterias == null || !SortCriterias.Any()) ? [DefaultSortCriteria] : SortCriterias;
                pagingRequest.SortCriterias = this.SortCriterias;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Store/SelectAllOnBoardCustomer?JobPositionUID={_appuser.SelectedJobPosition.UID}&Role={_appuser.Role.Code}",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.Store.Model.Classes.OnBoardGridview>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Store.Model.Classes.OnBoardGridview>>>(apiResponse.Data);
                    TotalItemsCount = pagedResponse.Data.TotalCount;
                    return pagedResponse.Data.PagedData.ToList<Winit.Modules.Store.Model.Interfaces.IOnBoardGridview>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
            return null;
        }
        private async Task<List<CommonUIDResponse>> SaveDocumentFileSysDataFromAPIAsync(List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> skufilesys, bool IsCreate)
        {
            try
            {
                string jsonBody = JsonConvert.SerializeObject(skufilesys);
                ApiResponse<string> apiResponse = null;
                if (IsCreate)
                {
                    apiResponse = await _apiService.FetchDataAsync(
                        $"{_appConfigs.ApiBaseUrl}FileSys/CreateFileSysForBulk", HttpMethod.Post, skufilesys);
                }
                else
                {
                    apiResponse = await _apiService.FetchDataAsync(
                        $"{_appConfigs.ApiBaseUrl}FileSys/CreateFileSysForBulk", HttpMethod.Put, skufilesys);
                }

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    // Assuming CommonUIDResponse is a class with a property 'UID'
                    IsCodeOfEthics = true;
                    List<CommonUIDResponse> commonUIDResponses = JsonConvert.DeserializeObject<List<CommonUIDResponse>>(data);
                    return commonUIDResponses;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null; // Or handle the case when no response is received
        }
        private async Task<bool> CreateUpdateDocumentFileSysDataAppendixFromAPIAsync(List<List<Winit.Modules.FileSys.Model.Interfaces.IFileSys>> skufilesys)
        {
            try
            {
                string jsonBody = JsonConvert.SerializeObject(skufilesys);
                ApiResponse<string> apiResponse = null;

                apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}FileSys/CreateFileSysForList", HttpMethod.Post, skufilesys);


                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    // Assuming CommonUIDResponse is a class with a property 'UID'
                    return Convert.ToBoolean(data);
                }
                if (apiResponse != null)
                {
                    string ErrorResponse = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data, "ErrorResponse");
                    if (ErrorResponse.Contains("PRIMARY"))
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false; // Or handle the case when no response is received
        }
        private async Task<int> DeleteOnBoardingDetailsGridiviewFromAPIAsync(string UID)
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Store/DeleteOnBoardingDetails?UID={UID}",
                    HttpMethod.Delete, UID);
            if (apiResponse != null && apiResponse.Data != null && apiResponse.StatusCode == 200)
            {
                string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                int result = JsonConvert.DeserializeObject<int>(data);
                return 1;
            }
            else
            {
                return 0;
            }
        }
        private async Task<int> DeleteAsmDivisionMappingFromAPIAsync(string UID)
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Store/DeleteAsmDivisionMapping?UID={UID}",
                    HttpMethod.Delete, UID);
            if (apiResponse != null && apiResponse.Data != null && apiResponse.StatusCode == 200)
            {
                string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                int result = JsonConvert.DeserializeObject<int>(data);
                return 1;
            }
            else
            {
                return 0;
            }
        }
        private async Task<List<Winit.Modules.Store.Model.Classes.Store>> GetBroadClassificationDetailsFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = PageNumber;
                //  pagingRequest.PageSize = PageSize;
                pagingRequest.SortCriterias = SortCriterias;
                FilterCriterias.Add(new FilterCriteria("IsActive", 1, FilterType.Equal));
                pagingRequest.FilterCriterias = FilterCriterias;
                pagingRequest.IsCountRequired = true;

                Winit.Shared.Models.Common.ApiResponse<string> BroadClassificationDetails = await
                    _apiService.FetchDataAsync
                    ($"{_appConfigs.ApiBaseUrl}BroadClassificationHeader/GetBroadClassificationHeaderDetails", HttpMethod.Post, pagingRequest);
                if (BroadClassificationDetails != null && BroadClassificationDetails.IsSuccess)
                {
                    ApiResponse<PagedResponse<Winit.Modules.Store.Model.Classes.Store>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Store.Model.Classes.Store>>>(BroadClassificationDetails.Data);
                    return pagedResponse.Data.PagedData.OfType<Winit.Modules.Store.Model.Classes.Store>().ToList();
                    //TotalCount = BroadClassificationDetails.Data.TotalCount;
                    //return BroadClassificationDetails.Data.PagedData.ToList<Model.Classes.Store>();
                }
                return default;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<IStoreCredit>> GetStoreCreditDetailsFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = PageNumber;
                //  pagingRequest.PageSize = PageSize;
                pagingRequest.SortCriterias = SortCriterias;
                pagingRequest.FilterCriterias = FilterCriterias;
                pagingRequest.IsCountRequired = true;

                Winit.Shared.Models.Common.ApiResponse<string> StoreCreditDetails = await
                    _apiService.FetchDataAsync
                    ($"{_appConfigs.ApiBaseUrl}StoreCredit/SelectAllStoreCredit", HttpMethod.Post, pagingRequest);
                if (StoreCreditDetails != null && StoreCreditDetails.IsSuccess)
                {
                    ApiResponse<PagedResponse<Winit.Modules.Store.Model.Classes.StoreCredit>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Store.Model.Classes.StoreCredit>>>(StoreCreditDetails.Data);
                    return pagedResponse.Data.PagedData.OfType<Winit.Modules.Store.Model.Interfaces.IStoreCredit>().ToList();
                    //TotalCount = BroadClassificationDetails.Data.TotalCount;
                    //return BroadClassificationDetails.Data.PagedData.ToList<Model.Classes.Store>();
                }
                return default;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<IBroadClassificationLine>> GetBroadClassificationLineDetailsFromAPIAsync(string UID)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<List<BroadClassificationLine>> BroadClassificationLineDetails = await
                    _apiService.FetchDataAsync<List<BroadClassificationLine>>
                    ($"{_appConfigs.ApiBaseUrl}BroadClassificationLine/GetBroadClassificationLineDetailsByUID?UID=" + UID, HttpMethod.Get, null);
                if (BroadClassificationLineDetails != null && BroadClassificationLineDetails.IsSuccess)
                {
                    return BroadClassificationLineDetails.Data.ToList<IBroadClassificationLine>();
                }
                return default;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<List<IEmp>> GetAllASMDetailsFromAPIAsync(string BranchUID)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<List<Winit.Modules.Emp.Model.Classes.Emp>> AsmDetails = await
                    _apiService.FetchDataAsync<List<Winit.Modules.Emp.Model.Classes.Emp>>
                    ($"{_appConfigs.ApiBaseUrl}Emp/GetASMList?branchUID=" + BranchUID, HttpMethod.Get, null);
                if (AsmDetails != null && AsmDetails.IsSuccess)
                {
                    return AsmDetails.Data.ToList<IEmp>();
                }
                return default;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<List<ILocation>> GetStateDetailsFromAPIAsync(List<string> locationTypes)
        {
            try
            {
                ApiResponse<List<Winit.Modules.Location.Model.Classes.Location>> apiResponse = await _apiService.FetchDataAsync
                 <List<Winit.Modules.Location.Model.Classes.Location>>(
                 $"{_appConfigs.ApiBaseUrl}Location/GetLocationByTypes",
                 HttpMethod.Post, locationTypes);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToList<ILocation>();
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<IOrg>> GetOUDetailsFromAPIAsync(string OrgType)
        {
            try
            {
                ApiResponse<List<Winit.Modules.Org.Model.Classes.Org>> apiResponse = await _apiService.FetchDataAsync
                 <List<Winit.Modules.Org.Model.Classes.Org>>(
                 $"{_appConfigs.ApiBaseUrl}Org/GetOrgByOrgTypeUID?OrgTypeUID=" + OrgType,
                 HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToList<IOrg>();
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<IOrg>> GetDivisionDetailsFromAPIAsync(string OrgType, string ParentUID)
        {
            try
            {
                ApiResponse<List<Winit.Modules.Org.Model.Classes.Org>> apiResponse = await _apiService.FetchDataAsync
                 <List<Winit.Modules.Org.Model.Classes.Org>>(
                 $"{_appConfigs.ApiBaseUrl}Org/GetOrgByOrgTypeUID?OrgTypeUID=" + OrgType + "&parentUID=" + ParentUID,
                 HttpMethod.Post);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToList<IOrg>();
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<IOrg>> GetAllDivisionDetailsFromAPIAsync()
        {
            try
            {
                ApiResponse<List<Winit.Modules.Org.Model.Classes.Org>> apiResponse = await _apiService.FetchDataAsync<List<Winit.Modules.Org.Model.Classes.Org>>(
                    $"{_appConfigs.ApiBaseUrl}Org/GetDivisions",
                    HttpMethod.Post, string.Empty);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToList<IOrg>();
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<ILocation> GetCountryAndRegionDetailsFromAPI(string UID, string Type)
        {
            try
            {
                ApiResponse<Winit.Modules.Location.Model.Classes.Location> apiResponse = await _apiService.FetchDataAsync
                 <Winit.Modules.Location.Model.Classes.Location>(
                 $"{_appConfigs.ApiBaseUrl}Location/GetCountryAndRegionByState?UID=" + UID + "&Type=" + Type,
                 HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<ILocation>> GetCityAndLocalityDetailsFromAPIAsync(List<string> uids)
        {
            try
            {
                ApiResponse<List<Winit.Modules.Location.Model.Classes.Location>> apiResponse = await _apiService.FetchDataAsync
                 <List<Winit.Modules.Location.Model.Classes.Location>>(
                 $"{_appConfigs.ApiBaseUrl}Location/GetCityandLoaclityByUIDs",
                 HttpMethod.Post, uids);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToList<ILocation>();
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<IBranch>> GetBranchDetailsFromAPIAsync(BranchHeirarchy locationTypes)
        {
            try
            {
                string Name = "";
                if (locationTypes != null)
                {
                    switch (locationTypes.Name)
                    {
                        case BranchHeirarchyConstants.Locality:
                            Name = locationTypes.Name;
                            break;
                        case BranchHeirarchyConstants.City:
                            Name = locationTypes.Name;
                            break;
                        case BranchHeirarchyConstants.State:
                            Name = locationTypes.Name;
                            break;
                        default:
                            break;
                    }
                }
                ApiResponse<List<Winit.Modules.Location.Model.Classes.Branch>> apiResponse = await _apiService.FetchDataAsync
             <List<Winit.Modules.Location.Model.Classes.Branch>>(
             $"{_appConfigs.ApiBaseUrl}Branch/GetBranchByLocationHierarchy?{Name}=" + locationTypes.UID,
             HttpMethod.Get, null);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToList<IBranch>();
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<List<IEmp>> GetASMDetailsFromBranchFromAPIAsync(string UID, string Code = null)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<List<Winit.Modules.Emp.Model.Classes.Emp>> ASMDetails = await
                    _apiService.FetchDataAsync<List<Winit.Modules.Emp.Model.Classes.Emp>>
                    ($"{_appConfigs.ApiBaseUrl}Emp/GetASMList?branchUID=" + UID, HttpMethod.Get, null);
                if (ASMDetails != null && ASMDetails.IsSuccess)
                {
                    return ASMDetails.Data.ToList<IEmp>();
                }
                return default;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<List<IEmp>> GetASEMDetailsFromBranchFromAPIAsync(string UID, string Code = null)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<List<Winit.Modules.Emp.Model.Classes.Emp>> ASEMDetails = await
                    _apiService.FetchDataAsync<List<Winit.Modules.Emp.Model.Classes.Emp>>
                    ($"{_appConfigs.ApiBaseUrl}Emp/GetASMList?branchUID=" + UID + "&Code=" + Code, HttpMethod.Get, null);
                if (ASEMDetails != null && ASEMDetails.IsSuccess)
                {
                    return ASEMDetails.Data.ToList<IEmp>();
                }
                return default;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<List<ISalesOffice>> GetSalesOfficeDetailsFromBranchFromAPIAsync(string UID)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<List<Winit.Modules.Location.Model.Classes.SalesOffice>> SalesOfficeDetails = await
                    _apiService.FetchDataAsync<List<Winit.Modules.Location.Model.Classes.SalesOffice>>
                    ($"{_appConfigs.ApiBaseUrl}SalesOffice/GetSalesOfficeByUID?UID={UID}", HttpMethod.Get, null);
                if (SalesOfficeDetails != null && SalesOfficeDetails.IsSuccess)
                {
                    return SalesOfficeDetails.Data.ToList<ISalesOffice>();
                }
                return default;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<Winit.Modules.Store.Model.Classes.Store> CheckExistOrNotFromAPIAsync(string UID)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<Winit.Modules.Store.Model.Classes.Store> BroadClassificationLineDetails = await
                    _apiService.FetchDataAsync<Winit.Modules.Store.Model.Classes.Store>
                    ($"{_appConfigs.ApiBaseUrl}Store/SelectStoreByUID?UID=" + UID, HttpMethod.Get, null);
                if (BroadClassificationLineDetails != null && BroadClassificationLineDetails.IsSuccess)
                {
                    return BroadClassificationLineDetails.Data;
                }
                return default;
            }
            catch (Exception)
            {
                throw;
            }
        }


        private async Task<List<IAllApprovalRequest>> GetApprovalStatusFromAPIAsync(string LinkedItemUID)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<List<AllApprovalRequest>> ApprovalStatusDetails = await
                    _apiService.FetchDataAsync<List<AllApprovalRequest>>
                    ($"{_appConfigs.ApiBaseUrl}Store/GetApprovalStatusByStoreUID?LinkItemUID=" + LinkedItemUID, HttpMethod.Get, null);
                if (ApprovalStatusDetails != null && ApprovalStatusDetails.IsSuccess)
                {
                    return ApprovalStatusDetails.Data.ToList<IAllApprovalRequest>();
                }
                return new List<IAllApprovalRequest>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<IOnBoardEditCustomerDTO> GetAllOnBoardingDetailsByStoreUIDFromAPIAsync(string UID)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<OnBoardEditCustomerDTO> OnBoardingEditDetails = await
                    _apiService.FetchDataAsync<OnBoardEditCustomerDTO>
                    ($"{_appConfigs.ApiBaseUrl}Store/GetAllOnBoardingDetailsByStoreUID?UID=" + UID, HttpMethod.Get, null);
                if (OnBoardingEditDetails != null && OnBoardingEditDetails.IsSuccess)
                {

                    return OnBoardingEditDetails.Data;
                }
                return default;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<List<IAsmDivisionMapping>> GetAsmDetailsByUIDFromAPIAsync(string LinkedItemType, string LinkedItemUID)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<List<AsmDivisionMapping>> AsmDivisionDetails = await
                    _apiService.FetchDataAsync<List<AsmDivisionMapping>>
                    ($"{_appConfigs.ApiBaseUrl}Store/GetAsmDivisionMappingByUID/{LinkedItemType}/{LinkedItemUID}", HttpMethod.Get, null);
                if (AsmDivisionDetails != null && AsmDivisionDetails.IsSuccess)
                {
                    return AsmDivisionDetails.Data.ToList<IAsmDivisionMapping>();
                }
                return default;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<List<IAllApprovalRequest>> GetAllApproveListDetailsFromAPIAsync(string UID)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<List<AllApprovalRequest>> AllApprovalLevelDetails = await
                    _apiService.FetchDataAsync<List<AllApprovalRequest>>
                    ($"{_appConfigs.ApiBaseUrl}Store/GetApprovalDetailsByStoreUID?LinkItemUID=" + UID, HttpMethod.Get, null);
                if (AllApprovalLevelDetails != null && AllApprovalLevelDetails.IsSuccess)
                {
                    return AllApprovalLevelDetails.Data.ToList<IAllApprovalRequest>();
                }
                return default;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<string> DeleteContactDetailsFromGridFromAPIAsync(string uid)
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Contact/DeleteContactDetails?UID={uid}",
                    HttpMethod.Delete, uid);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return "Contact Deleted Successfully";
                }
                else if (apiResponse != null && apiResponse.Data != null)
                {
                    ApiResponse<string> data = JsonConvert.DeserializeObject<ApiResponse<string>>(apiResponse.Data);
                    return $"Error to Delete : {data.ErrorMessage}";
                }
                else
                {
                    return "an Unexcepted Error Occured";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<string> DeleteAddressDetailsFromGridFromAPIAsync(string uid)
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Address/DeleteAddressDetails?UID={uid}",
                    HttpMethod.Delete, uid);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return "Address Deleted Successfully";
                }
                else if (apiResponse != null && apiResponse.Data != null)
                {
                    ApiResponse<string> data = JsonConvert.DeserializeObject<ApiResponse<string>>(apiResponse.Data);
                    return $"Error to Delete : {data.ErrorMessage}";
                }
                else
                {
                    return "an Unexcepted Error Occured";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task GetRuleIdFromAPIAsync(string type, string typeCode)
        {

            try
            {

                ApiResponse<string>? apiResponse = null;

                apiResponse = await _apiService.FetchDataAsync(
              $"{_appConfigs.ApiBaseUrl}ApprovalEngine/GetRuleId?type={type}&typeCode={typeCode}", HttpMethod.Get);

                if (apiResponse != null && apiResponse.Data != "0")
                {
                    ApiResponse<string> data = JsonConvert.DeserializeObject<ApiResponse<string>>(apiResponse.Data);
                    RuleId = int.Parse(data.Data);
                }

            }
            catch (Exception)
            {
                throw;
            }

        }
        private async Task GetAllUserHierarchyFromAPIAsync(string hierarchyType, string hierarchyUID, int ruleId)
        {

            try
            {
                Winit.Shared.Models.Common.ApiResponse<List<UserHierarchy>> apiResponse = await
                    _apiService.FetchDataAsync<List<UserHierarchy>>
                    ($"{_appConfigs.ApiBaseUrl}MaintainUser/GetUserHierarchyForRule/{hierarchyType}/{hierarchyUID}/{ruleId}", HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data is not null)
                {
                    UserRole_Code = new Dictionary<string, List<string>>();

                    foreach (var item in apiResponse.Data)
                    {
                        string roleCode = item.RoleCode;
                        string userCode = item.EmpCode;
                        if (UserRole_Code.ContainsKey(roleCode))
                        {
                            UserRole_Code[roleCode].Add(userCode);
                        }
                        else
                        {
                            UserRole_Code[roleCode] = new List<string> { userCode };
                        }
                    }

                }
            }
            catch (Exception)
            {

                throw;
            }

        }
        public override async Task<IEmp> GetBMByBranchUID(string UID)
        {

            try
            {
                Winit.Shared.Models.Common.ApiResponse<IEmp> apiResponse = await
                    _apiService.FetchDataAsync<IEmp>
                    ($"{_appConfigs.ApiBaseUrl}Emp/GetBMByBranchUID?UID={UID}", HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data is not null)
                {
                    return apiResponse.Data;
                }
                return default;
            }
            catch (Exception)
            {
                throw;
            }

        }
        private async Task<bool> SaveApprovalRequestDetailsFromAPIAsync(string RequestId)
        {

            try
            {
                ApiResponse<string>? apiResponse = null;
                var allApprovalRequest = new AllApprovalRequest
                {
                    LinkedItemType = "store",
                    LinkedItemUID = TabName == StoreConstants.Confirmed ? ChangeRequestUid : StoreUID,
                    RequestID = RequestId,
                    ApprovalUserDetail = JsonConvert.SerializeObject(UserRole_Code)
                };

                apiResponse = await _apiService.FetchDataAsync(
              $"{_appConfigs.ApiBaseUrl}Store/CreateAllApprovalRequest", HttpMethod.Post, allApprovalRequest);

                if (apiResponse != null)
                {
                    return apiResponse.IsSuccess;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;

        }
        private async Task<bool> CreateUpdateEmployeeDetailsFromAPIAsync(IStoreAdditionalInfoCMI storeAdditionalInfoCMI)
        {
            try
            {
                ApiResponse<string>? apiResponse = null;
                //    if (IsCreate)
                //    {
                //        AddCreateFields(storeAdditionalInfoCMI, false);
                //        string jsonBody = JsonConvert.SerializeObject(storeAdditionalInfoCMI);
                //        apiResponse = await _apiService.FetchDataAsync(
                //$"{_appConfigs.ApiBaseUrl}StoreAdditionalInfoCMI/CreateStoreAdditionalInfoCMI", HttpMethod.Post, storeAdditionalInfoCMI);
                //    }
                //    else
                //    {
                AddUpdateFields(storeAdditionalInfoCMI);
                apiResponse = await _apiService.FetchDataAsync(
        $"{_appConfigs.ApiBaseUrl}StoreAdditionalInfoCMI/UpdateStoreAdditionalInfoCMI", HttpMethod.Put, storeAdditionalInfoCMI);
                // }

                if (apiResponse != null)
                {
                    return apiResponse.IsSuccess;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
        private async Task<bool> CreateUpdateDistDetailsFromAPIAsync(StoreApprovalDTO storeApprovalDTO)
        {
            try
            {
                ApiResponse<string>? apiResponse = null;
                //    if (IsCreate)
                //    {
                //        AddCreateFields(storeAdditionalInfoCMI, false);
                //        string jsonBody = JsonConvert.SerializeObject(storeAdditionalInfoCMI);
                //        apiResponse = await _apiService.FetchDataAsync(
                //$"{_appConfigs.ApiBaseUrl}StoreAdditionalInfoCMI/CreateStoreAdditionalInfoCMI", HttpMethod.Post, storeAdditionalInfoCMI);
                //    }
                //    else
                //    {

                //{ These code written by Aziz
                if (!storeApprovalDTO.Store.IsApprovalCreated)
                {
                    storeApprovalDTO.ApprovalRequestItem = PrepareApprovalRequestItem(storeApprovalDTO.Store);
                }
                AddUpdateFields(storeApprovalDTO.Store);
                apiResponse = await _apiService.FetchDataAsync(
              $"{_appConfigs.ApiBaseUrl}Store/UpdateStoreStatus", HttpMethod.Put, storeApprovalDTO);
                // }

                if (apiResponse != null)
                {
                    return apiResponse.IsSuccess;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
        private ApprovalRequestItem? PrepareApprovalRequestItem(IStore store)
        {
            ApprovalRequestItem approvalRequestItem = new ApprovalRequestItem();
            approvalRequestItem.HierarchyType = UserHierarchyTypeConst.Emp;
            approvalRequestItem.HierarchyUid = ReportingUID();
            // approvalRequestItem.RuleId = BroadClassfication == "Service" ? GetRuleIdByName(ApprovalParameterConstants.RuleNameForServiceBroadClassification) : GetRuleIdByName(ApprovalParameterConstants.RuleNameForOtherThanServiceBroadClassification);
            //approvalRequestItem.RuleId = (BroadClassfication == "Service" || BroadClassfication == "CP-AU SERVICE PARTNER")
            //                             ? GetRuleIdByName(ApprovalParameterConstants.RuleNameForServiceBroadClassification)
            //                             : GetRuleIdByName(ApprovalParameterConstants.RuleNameForOtherThanServiceBroadClassification);

            approvalRequestItem.RuleId = BroadClassfication switch
            {
                "Service" => GetRuleIdByName(ApprovalParameterConstants.RuleNameForTrader),
                "MT" => GetRuleIdByName(ApprovalParameterConstants.RuleNameForMT),
                "E COM" => GetRuleIdByName(ApprovalParameterConstants.RuleNameForECOM),

                "Dist" or "RETAIL" or "LFS" => GetRuleIdByName(ApprovalParameterConstants.RuleNameForDistributorRetailerLFS),

                "CP-AU SERVICE PARTNER" => GetRuleIdByName(ApprovalParameterConstants.RuleNameForService), //its nothing but trader  RuleNameForService

                "SSD" => GetRuleIdByName(ApprovalParameterConstants.RuleNameForSSD),

                _ => GetRuleIdByName(ApprovalParameterConstants.RuleNameForOtherThanServiceBroadClassification)
            };

            string ruleParam = BroadClassfication switch
            {
                "Service" => ApprovalParameterConstants.RuleParamForTrader,
                "MT" => ApprovalParameterConstants.RuleParamForMT,
                "E COM" => ApprovalParameterConstants.RuleParamForECOM,
                "Dist" or "RETAIL" or "LFS" or "MT" or "E COM" => ApprovalParameterConstants.RuleParamForDistributorRetailerLFS,

                "CP-AU SERVICE PARTNER" => ApprovalParameterConstants.RuleParamForService, //its nothing but trader

                "SSD" => ApprovalParameterConstants.RuleParamForSSD,

                _ => string.Empty
            };


            approvalRequestItem.Payload = new Dictionary<string, object>
            {
                { "RequesterId", _appuser?.Emp?.Code ?? ""},
                {
                 "UserRoleCode" , _appuser?.Role?.Code ?? ""
                 },
                { "Remarks", "Need approval" },
                { "Customer", new ApprovalEngine.Model.Classes.Customer { BroadCustomerClassification = ruleParam} }
            };
            return approvalRequestItem;
        }
        public int GetRuleIdByName(string ruleName)
        {
            try
            {
                return _appuser?.ApprovalRuleMaster?.Where(item => item.RuleName == ruleName).Select(item => item.RuleId).FirstOrDefault() ?? 0;
            }
            catch (Exception ex)
            {
                return default;
            }
        }
        public string ReportingUID()
        {
            try
            {
                if (string.IsNullOrEmpty(SelfRegistrationUID ?? EditOnBoardingDetails?.StoreAdditionalInfoCMI?.SelfRegistrationUID))
                {
                    return _appuser.Emp.UID;
                }
                return EditOnBoardingDetails?.Store?.ReportingEmpUID!;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }


        }
        public override async Task<bool> CreateDistributorAfterFinalApproval(IStore store)
        {
            try
            {
                string TaxGroupId = "GST India";
                IOrg org = new Winit.Modules.Org.Model.Classes.Org()
                {
                    UID = store.UID,
                    Code = store.Code,
                    Name = store.Name,
                    IsActive = true,
                    OrgTypeUID = "FR",
                    ParentUID = store.FranchiseeOrgUID,
                    TaxGroupUID = TaxGroupId,
                    Status = "Active",
                    CreatedBy = _appuser?.Emp?.UID ?? "ADMIN",
                    ModifiedBy = _appuser?.Emp?.UID ?? "ADMIN",
                    CreatedTime = DateTime.Now,
                    ModifiedTime = DateTime.Now,
                };


                Winit.Modules.Int_CommonMethods.Model.Interfaces.IPendingDataRequest pendingRequestData = new PendingDataRequest
                {
                    LinkedItemUid = store.UID,
                    LinkedItemType = "Store",
                    Status = "Pending"
                };



                ApiResponse<string>? apiResponse = await _apiService.FetchDataAsync(
        $"{_appConfigs.ApiBaseUrl}Org/CreateOrg", HttpMethod.Post, org);

                ApiResponse<string>? apiResponseOrgHeirarchy = await _apiService.FetchDataAsync(
        $"{_appConfigs.ApiBaseUrl}Org/InsertOrgHierarchy", HttpMethod.Post);

                ApiResponse<string>? apiResponseIntegration = await _apiService.FetchDataAsync(
       $"{_appConfigs.ApiBaseUrl}IntPendingDataInsertion/InsertPendingData", HttpMethod.Post, pendingRequestData);



                if (apiResponse != null && apiResponseOrgHeirarchy != null && await CreateWareHouses(store))
                {
                    ApiResponse<string>? apiResponseGenerateMyTeam = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Store/GenerateMyTeam", HttpMethod.Post);
                    return apiResponse.IsSuccess;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
        public async Task<bool> CreateWareHouses(IStore store)
        {
            string TaxGroupId = "GST India";
            List<IOrg> warehouseOrgs = new List<IOrg>();
            try
            {
                List<IAddress> addresses = await GetAllAddressDataFromAPIAsync("Shipping");
                foreach (var address in addresses)
                {
                    IOrg wareHouseOrg = new Winit.Modules.Org.Model.Classes.Org()
                    {
                        UID = address.UID,          // Assign UID from the address object
                        Code = address.UID,      // Assuming address has ZipCode
                        Name = address.UID,         // Assuming address has Name
                        IsActive = true,
                        ParentUID = store.UID,       // Assuming 'store' has UID
                        OrgTypeUID = "FRWH",
                        Status = "Active",
                        TaxGroupUID = TaxGroupId,    // Assuming TaxGroupId is available
                        CreatedBy = _appuser?.Emp?.UID ?? "ADMIN",
                        ModifiedBy = _appuser?.Emp?.UID ?? "ADMIN",
                        CreatedTime = DateTime.Now,
                        ModifiedTime = DateTime.Now
                    };

                    warehouseOrgs.Add(wareHouseOrg);
                }

                ApiResponse<string>? warehouse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}Org/CreateOrgBulk", HttpMethod.Post, warehouseOrgs);

                if (warehouse != null && warehouse != null)
                {
                    return warehouse.IsSuccess;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private async Task<bool> CreateUpdatecontactFromAPIAsync(IContact contact, bool IsCreate)
        {
            try
            {
                ApiResponse<string>? apiResponse = null;
                if (IsCreate)
                {
                    AddCreateFields(contact, false);
                    string jsonBody = JsonConvert.SerializeObject(contact);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}Contact/CreateContactDetails", HttpMethod.Post, contact);
                }
                else
                {
                    AddUpdateFields(contact);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}Contact/UpdateContactDetails", HttpMethod.Put, contact);
                }

                if (apiResponse != null)
                {
                    return apiResponse.IsSuccess;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
        private async Task<bool> CreateUpdateCustomerInformationFromAPIAsync(IOnBoardCustomerDTO onBoardCustomerDTO, bool IsCreate)
        {
            try
            {
                ApiResponse<string> apiResponse = null;
                if (IsCreate)
                {
                    string jsonBody = JsonConvert.SerializeObject(onBoardCustomerDTO);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}Store/CUDOnBoardCustomerInfo", HttpMethod.Post, onBoardCustomerDTO);
                }
                else
                {
                    string jsonBody = JsonConvert.SerializeObject(onBoardCustomerDTO);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}Store/CUDOnBoardCustomerInfo", HttpMethod.Put, onBoardCustomerDTO);
                }

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    if (apiResponse.IsSuccess != null && apiResponse.IsSuccess)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
        private async Task<bool> CreateUpdateaddressFromAPIAsync(IAddress address, bool IsCreate)
        {
            try
            {
                ApiResponse<string>? apiResponse = null;
                if (IsCreate)
                {
                    AddCreateFields(address, false);
                    string jsonBody = JsonConvert.SerializeObject(address);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}Address/CreateAddressDetails", HttpMethod.Post, address);
                }
                else
                {
                    AddUpdateFields(address);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}Address/UpdateAddressDetails", HttpMethod.Put, address);
                }

                if (apiResponse != null)
                {
                    return apiResponse.IsSuccess;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
        private async Task<bool> CreateGstAddressFromAPIAsync(List<IAddress> address)
        {
            try
            {
                ApiResponse<string>? apiResponse = null;
                address.ForEach(p => p.CreatedBy = _appuser?.Emp?.UID ?? "ADMIN");
                address.ForEach(p => p.ModifiedBy = _appuser?.Emp?.UID ?? "ADMIN");
                address.ForEach(p => p.CreatedTime = DateTime.Now);
                address.ForEach(p => p.ModifiedTime = DateTime.Now);
                address.ForEach(p => p.LinkedItemType = "store");
                address.ForEach(p => p.LinkedItemUID = StoreUID);
                address.ForEach(p => p.UID = Guid.NewGuid().ToString());
                string jsonBody = JsonConvert.SerializeObject(address);
                apiResponse = await _apiService.FetchDataAsync(
        $"{_appConfigs.ApiBaseUrl}Address/CreateAddressDetailsList", HttpMethod.Post, address);

                if (apiResponse != null)
                {
                    return apiResponse.IsSuccess;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
        private async Task<bool> CreateUpdateAsmDivisionMappingFromAPIAsync(List<IAsmDivisionMapping> asmDivisionMapping, bool IsCreate)
        {
            try
            {
                ApiResponse<string>? apiResponse = null;
                if (IsCreate)
                {
                    asmDivisionMapping.ForEach(p => p.CreatedBy = _appuser?.Emp?.UID ?? "ADMIN");
                    asmDivisionMapping.ForEach(p => p.ModifiedBy = _appuser?.Emp?.UID ?? "ADMIN");
                    asmDivisionMapping.ForEach(p => p.CreatedTime = DateTime.Now);
                    asmDivisionMapping.ForEach(p => p.ModifiedTime = DateTime.Now);
                    asmDivisionMapping.ForEach(p => p.ServerAddTime = DateTime.Now);
                    asmDivisionMapping.ForEach(p => p.ServerModifiedTime = DateTime.Now);
                }
                else
                {
                    asmDivisionMapping.ForEach(p => p.ModifiedBy = _appuser?.Emp?.UID ?? "ADMIN");
                    asmDivisionMapping.ForEach(p => p.ModifiedTime = DateTime.Now);
                }
                apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}Store/CreateAsmDivisionMapping", HttpMethod.Post, asmDivisionMapping);
                if (apiResponse != null)
                {
                    return apiResponse.IsSuccess;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
        private async Task<List<IContact>> GetAllContactDataFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest
                {
                    IsCountRequired = true,
                    FilterCriterias = new List<FilterCriteria>
            {
                new FilterCriteria(
                    name: "Type",
                    value: new List<string> { "Residence", "Office",null },
                    type: FilterType.In
                ),
                 new FilterCriteria(
                    name: "LinkedItemUID",
                    value:StoreUID,
                    type: FilterType.Equal
                )
            }
                };
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(

                    $"{_appConfigs.ApiBaseUrl}Contact/SelectAllContactDetails",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.Contact.Model.Classes.Contact>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Contact.Model.Classes.Contact>>>(apiResponse.Data);
                    return pagedResponse.Data.PagedData.OfType<IContact>().ToList();
                }
            }
            catch (Exception)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        private async Task<List<IAddress>> GetAllAddressDataFromAPIAsync(string Type)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest
                {
                    IsCountRequired = true,
                    FilterCriterias = new List<FilterCriteria>
    {
        new FilterCriteria(
            name: "Type",
            value: new List<string> { Type },
            type: FilterType.In
        ),
        new FilterCriteria(
                        name: "LinkedItemType",
            value: new List<string> { "store" },
            type: FilterType.In
        ),
        new FilterCriteria(
            name: "LinkedItemUID",
            value: new List<string> { StoreUID },
            type: FilterType.In
        )
    }
                };

                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(

                    $"{_appConfigs.ApiBaseUrl}Address/SelectAllAddressDetails",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.Address.Model.Classes.Address>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Address.Model.Classes.Address>>>(apiResponse.Data);
                    return pagedResponse.Data.PagedData.OfType<IAddress>().ToList();
                }
            }
            catch (Exception)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }

        public override async Task<bool> SaveAndUpdateSelfRegistration(ISelfRegistration selfRegistration)
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync<string>(
                   $"{_appConfigs.ApiBaseUrl}SelfRegistration/CrudSelfRegistration",
                   HttpMethod.Post, selfRegistration);

            return apiResponse != null && apiResponse.IsSuccess;
        }
        public override async Task<Dictionary<string, int>> GetTabItemsCountFromApi(List<FilterCriteria> filterCriterias)
        {
            try
            {
                ApiResponse<Dictionary<string, int>> apiResponse =
                await _apiService.FetchDataAsync<Dictionary<string, int>>(
                $"{_appConfigs.ApiBaseUrl}Store/GetTabsCount?JobPositionUID={_appuser.SelectedJobPosition.UID}&Role={_appuser.Role.Code}",
                HttpMethod.Post,
                filterCriterias
                );
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
                return [];
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public override async Task<bool> HandleVerifyOTP(ISelfRegistration selfRegistration)
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync<string>(
                $"{_appConfigs.ApiBaseUrl}SelfRegistration/VerifyOtp",
                HttpMethod.Post, selfRegistration);

            if (apiResponse != null && apiResponse.IsSuccess)
            {
                selfRegistration.UID = apiResponse.Data;
                return true;
            }

            return false;
        }
        #region Change Record Request Logic


        public string ChangeRequestUid { get; set; }

        public async Task<bool> InsertDataInChangeRequestTableAsync(string changeRecordDTOJson)
        {
            ChangeRequestDTO changeRequestDTO = new ChangeRequestDTO();
            List<ChangeRecordDTO> ChangeRecordDTOs = JsonConvert.DeserializeObject<List<ChangeRecordDTO>>(changeRecordDTOJson);
            //string changeRecordJson = CommonFunctions.ConvertToJson(changeRecordDTO);
            ////string changeRecordJson = GetChangedDataInJson(changeRequestData);
            changeRequestDTO.ChangeRequest = new ChangeRequest()
            {
                CreatedBy = _appuser.Emp.Name,
                UID = Guid.NewGuid().ToString(),
                ApprovedDate = CustomerEditApprovalRequired == false ? DateTime.Now : null,
                EmpUid = _appuser.Emp.UID,
                LinkedItemUid = EditOnBoardingDetails?.Store?.UID!,
                ChannelPartnerCode = EditOnBoardingDetails?.Store?.Code!,
                ChannelPartnerName = EditOnBoardingDetails?.Store?.Name!,
                LinkedItemType = StoreConstants.Store,
                RequestDate = DateTime.Now,
                OperationType=ChangeRecordDTOs[0].Action,
                Reference=ChangeRecordDTOs[0].ScreenModelName,
                Status = CustomerEditApprovalRequired == false ? StoreConstants.Approved : StoreConstants.Pending,
                ChangeData = changeRecordDTOJson,
            };
            ChangeRequestUid = changeRequestDTO.ChangeRequest.UID;
            changeRequestDTO.ApprovalRequestItem=PrepareApprovalRequestItem(EditOnBoardingDetails?.Store!);
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}Store/CreateChangeRequest", HttpMethod.Post, changeRequestDTO);

            //force to garbage collector
            changeRequestDTO = null;
            ChangeRecordDTOs.Clear();
            changeRecordDTOJson = null;
            if (apiResponse.IsSuccess)
            {
                NewlyGeneratedUID = null;
                //_toast.Add("Success", "Changes Saved", UIComponents.SnackBar.Enum.Severity.Success);
                return true;
            }
            else
            {
                //_toast.Add("Error", "Failed to save the changes", UIComponents.SnackBar.Enum.Severity.Error);
                return false;
            }

        }

        //public string GetChangedDataInJson(ChangeRequestData changeRequestData)
        //{
        //    var changes = Winit.Shared.CommonUtilities.Common.CommonFunctions.CompareObjects(changeRequestData.OriginalObj, changeRequestData.ModifiedObj);
        //    ChangeRecord=new List<ChangeRecord>();

        //    foreach (var change in changes)
        //    {
        //        ChangeRecord.Add(new ChangeRecord
        //        {
        //            SectionSerialNo = changeRequestData.SectionSerialNumber,
        //            SectionName = changeRequestData.SectionName,
        //            FieldName = change.Key,
        //            OldValue = change.Value.OldValue,
        //            NewValue = change.Value.NewValue,
        //        });
        //    }
        //    string changeRecordJson = CommonFunctions.ConvertToJson(ChangeRecord);
        //    return changeRecordJson;
        //}
        #endregion
    }
}
