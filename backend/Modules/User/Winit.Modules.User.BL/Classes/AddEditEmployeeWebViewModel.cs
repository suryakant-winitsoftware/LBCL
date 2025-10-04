using Newtonsoft.Json;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.JobPosition.Model.Interfaces;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.User.Model.Classes;
using Winit.Modules.User.Model.Constants;
using Winit.Modules.User.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.SnackBar;
using Winit.UIComponents.SnackBar.Enum;
using Winit.UIModels.Common;
namespace Winit.Modules.User.BL.Classes
{
    public class AddEditEmployeeWebViewModel : AddEditEmployeeBaseViewModel
    {
        protected CommonFunctions _commonFunctions;
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private Winit.Modules.Common.BL.Interfaces.IAppUser _appuser;
        private IToast _toast;

        public AddEditEmployeeWebViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IAppUser appuser,
            IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService, IToast toast)
        : base(serviceProvider, filter, sorter, appuser, listHelper, appConfigs, apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _appuser = appuser;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _toast = toast;
            _commonFunctions = new CommonFunctions();

        }
        //public override async Task PopulateViewModel(string UID, bool IsNew)
        //{
        //    await base.PopulateViewModel(UID, IsNew);
        //}
        public override async Task PopulateViewModelForOrg_RoleMapping(string UID, bool IsNew)
        {
            await base.PopulateViewModelForOrg_RoleMapping(UID, IsNew);
        }
        public override async Task<List<IListItem>> GetAuthTypeDD()
        {
            return await GetAuthTypeDDDataFromAPIAsync();
        }
        public override async Task<List<IListItem>> GetDepartmentDD()
        {
            return await GetDepartmentDDDataFromAPIAsync();
        }
        //public override async Task<List<IOrg>> GetOrgDD()
        //{
        //    return await GetOrgDDDataFromAPIAsync();

        //}
        public override async Task<List<IRole>> GetRoleDD(string? UID)
        {
            return await GetRoleDDDataFromAPIAsync(UID);

        }
        public override async Task<List<IEmp>> GetReportsToDD(string? UID)
        {
            return await GetReportsToDDDataFromAPIAsync(UID);

        }
        public override async Task<List<IOrg>> GetEmpOrgMappingToDD(string empuid)
        {
            return await GetEmpOrgMappingToDDDataFromAPIAsync(empuid);
        }

        public override async Task<bool> CreateUpdateOrg_RoleMapping(Winit.Modules.JobPosition.Model.Interfaces.IJobPosition jobPosition, bool IsCreate)
        {
            return await CreateUpdateOrg_RoleMappingFromAPIAsync(jobPosition, IsCreate);
        }
        public override async Task<bool> CreateUpdateUser(IEmpDTO user, bool Iscreate)
        {
            return await CreateUpdateUserFromAPIAsync(user, Iscreate);
        }
        public override async Task<bool> CreateUpdateEmpOrgMapping(List<EmpOrgMapping> empOrgMapping)
        {
            return await CreateUpdateEmpOrgMappingFromAPIAsync(empOrgMapping);
        }
        public override async Task<string> DeleteEmpOrgMappingFromGrid(string uid)
        {
            return await DeleteEmpOrgMappingFromGridFromAPIAsync(uid);
        }
        public override async Task<(IEmpDTO empDTO, IJobPosition jobPosition)> GetUsersDetailsforEdit(string uid)
        {
            return await GetUsersDetailsforEditDataFromAPIAsync(uid);
        }

        public override async Task<List<ILocationType>> GetUserLocationTypes()
        {
            return await GetUserLocationTypesFromApiAsync();
        }
        public override async Task<List<ILocation>> GetUserLocationValues(string? code)
        {
            return await GetUserLocationValuesFromApiAsync(code);
        }



        public override async Task<bool> CreateUpdateUserLocationTypeAndValue(ILocationTypeAndValue locationTypeAndValueForLocationMapping)
        {
            return await CreateUpdateUserLocationTypeAndValueFromAPIAsync(locationTypeAndValueForLocationMapping);
        }
        public override async Task<ILocationTypeAndValue> GetUserLocationTypeAndValue(string uid)
        {
            return await GetUserLocationTypeAndValueFromApiAsync(uid);
        }

        private async Task<ILocationTypeAndValue> GetUserLocationTypeAndValueFromApiAsync(string uid)
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}MaintainUser/SelectMaintainUserDetailsByUID?empUID={uid}",
                    HttpMethod.Get);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<Winit.Modules.User.Model.Classes.EmpDTOModel> empDTOResponse = JsonConvert.DeserializeObject<ApiResponse<Winit.Modules.User.Model.Classes.EmpDTOModel>>(apiResponse.Data);

                    if (empDTOResponse != null && empDTOResponse.IsSuccess)
                    {
                        var empDTOData = empDTOResponse.Data;
                        IEmpDTO empDTO = new EmpDTO
                        {
                            Emp = empDTOData.Emp ?? new Winit.Modules.Emp.Model.Classes.Emp(),
                            EmpInfo = empDTOData.EmpInfo ?? new EmpInfo(),
                            JobPosition = empDTOData.JobPosition ?? new Winit.Modules.JobPosition.Model.Classes.JobPosition(),
                            EmpOrgMapping = empDTOData.EmpOrgMapping.ToList<IEmpOrgMapping>() ?? new List<IEmpOrgMapping>(),
                            FileSys = empDTOData.FileSys,

                        };
                        ILocationTypeAndValue loc = new LocationTypeAndValue
                        {
                            LocationType = empDTOData.JobPosition.LocationType,
                            LocationValue = empDTOData.JobPosition.LocationValue,
                        };
                        jobPosition = jobPosition ?? new Winit.Modules.JobPosition.Model.Classes.JobPosition();
                        return (loc);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        private async Task<bool> CreateUpdateUserLocationTypeAndValueFromAPIAsync(ILocationTypeAndValue locationTypeAndValueForLocationMapping)
        {
            try
            {
                JobPosition.UID = EmpDTOmaintainUser.JobPosition.UID;
                JobPosition.LocationValue = locationTypeAndValueForLocationMapping.LocationValue;
                JobPosition.LocationType = locationTypeAndValueForLocationMapping.LocationType;
                JobPosition.EmpCode=EmpDTOmaintainUser.Emp.Code;
                JobPosition.CreatedBy=_appuser.Emp.CreatedBy;
                ApiResponse<string> apiResponse =
                    await _apiService.FetchDataAsync<string>(
                    $"{_appConfigs.ApiBaseUrl}JobPosition/UpdateJobPositionLocationTypeAndValue",                     //  UpdateJobPosition
                    HttpMethod.Put, JobPosition);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<List<ILocationType>> GetUserLocationTypesFromApiAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest()
                {
                    PageNumber = 1,
                    PageSize = 10000,
                    IsCountRequired = true,
                };
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}LocationType/SelectAllLocationTypeDetails", HttpMethod.Post, pagingRequest);
                if (apiResponse != null)
                {
                    if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                    {
                        PagedResponse<LocationType>? pagedResponse = JsonConvert.DeserializeObject<PagedResponse<LocationType>>(_commonFunctions.GetDataFromResponse(apiResponse.Data));
                        if (pagedResponse != null)
                        {
                            return pagedResponse.PagedData.ToList<ILocationType>();
                        }
                        return null;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        private async Task<List<ILocation>> GetUserLocationValuesFromApiAsync(string? code)
        {
            try
            {
                List<string> locations = new List<string>()
                {
                    code,
                };
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}Location/GetLocationByTypes", HttpMethod.Post, locations);
                if (apiResponse != null)
                {
                    if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                    {
                        ApiResponse<List<Winit.Modules.Location.Model.Classes.Location>>? pagedResponse = JsonConvert.DeserializeObject<ApiResponse<List<Winit.Modules.Location.Model.Classes.Location>>>(apiResponse.Data);
                        if (pagedResponse != null)
                        {
                            return pagedResponse.Data.ToList<ILocation>();
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //protected override async Task<List<ILocationType>> GetUserLocationTypesFromApiAsync()
        //{
        //    try
        //    {
        //        PagingRequest pagingRequest = new PagingRequest()
        //        {
        //            PageNumber = 1,
        //            PageSize = 10000,
        //            IsCountRequired = true,
        //        };
        //        ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}LocationType/SelectAllLocationTypeDetails", HttpMethod.Post, pagingRequest);
        //        if (apiResponse != null)
        //        {
        //            if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
        //            {
        //                PagedResponse<LocationType>? pagedResponse = JsonConvert.DeserializeObject<PagedResponse<LocationType>>(_commonFunctions.GetDataFromResponse(apiResponse.Data));
        //                if (pagedResponse != null)
        //                {
        //                     return pagedResponse.PagedData.ToList<ILocationType>();
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}


        private async Task<(IEmpDTO empDTO, IJobPosition jobPosition)> GetUsersDetailsforEditDataFromAPIAsync(string uid)
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}MaintainUser/SelectMaintainUserDetailsByUID?empUID={uid}",
                    HttpMethod.Get);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<Winit.Modules.User.Model.Classes.EmpDTOModel> empDTOResponse = JsonConvert.DeserializeObject<ApiResponse<Winit.Modules.User.Model.Classes.EmpDTOModel>>(apiResponse.Data);

                    if (empDTOResponse != null && empDTOResponse.IsSuccess)
                    {
                        var empDTOData = empDTOResponse.Data;
                        IEmpDTO empDTO = new EmpDTO
                        {
                            Emp = empDTOData.Emp ?? new Winit.Modules.Emp.Model.Classes.Emp(),
                            EmpInfo = empDTOData.EmpInfo ?? new EmpInfo(),
                            JobPosition = empDTOData.JobPosition ?? new Winit.Modules.JobPosition.Model.Classes.JobPosition(),
                            EmpOrgMapping = empDTOData.EmpOrgMapping.ToList<IEmpOrgMapping>() ?? new List<IEmpOrgMapping>(),
                            FileSys = empDTOData.FileSys
                        };
                        Uid = empDTO.Emp.UID;
                        IJobPosition jobPosition = null;
                        if (empDTOData.JobPosition != null)
                        {
                            jobPosition = new Winit.Modules.JobPosition.Model.Classes.JobPosition
                            {
                                //OrgUID = empDTOData.JobPosition?.OrgUID,
                                UserRoleUID = empDTOData.JobPosition?.UserRoleUID,
                                ReportsToUID = empDTOData.JobPosition?.ReportsToUID,
                                Department = empDTOData.JobPosition?.Department,
                                CollectionLimit = empDTOData.JobPosition.CollectionLimit,
                                BranchUID = empDTO.JobPosition.BranchUID,
                                SalesOfficeUID = empDTO.JobPosition.SalesOfficeUID,
                            };
                        }

                        await GetApprovalDetails(uid);
                        jobPosition = jobPosition ?? new Winit.Modules.JobPosition.Model.Classes.JobPosition();
                        return (empDTO, jobPosition);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return (null, null);
        }
        private async Task GetApprovalDetails(string linkedItemUID)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<Winit.Modules.ApprovalEngine.Model.Classes.AllApprovalRequest> AllApprovalLevelDetails = await
                    _apiService.FetchDataAsync<Winit.Modules.ApprovalEngine.Model.Classes.AllApprovalRequest>
                    ($"{_appConfigs.ApiBaseUrl}ApprovalEngine/GetApprovalDetailsByLinkedItemUid?requestUid={linkedItemUID}", HttpMethod.Get);
                if (AllApprovalLevelDetails != null && AllApprovalLevelDetails.IsSuccess)
                {
                    AllApprovalRequestData = AllApprovalLevelDetails?.Data! as Winit.Modules.ApprovalEngine.Model.Interfaces.IAllApprovalRequest;
                    var approvalUserDetail = AllApprovalRequestData?.ApprovalUserDetail;
                    ApprovalUserCodes = string.IsNullOrEmpty(approvalUserDetail)
                    ? new Dictionary<string, List<EmployeeDetail>>()
                    : DeserializeApprovalUserCodes(approvalUserDetail) ?? new Dictionary<string, List<EmployeeDetail>>();
                }

            }
            catch (Exception)
            {
                throw;
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
        private async Task<string> DeleteEmpOrgMappingFromGridFromAPIAsync(string uid)
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}EmpOrgMapping/DeleteEmpOrgMapping?UID={uid}",
                    HttpMethod.Delete, uid);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return "EmpOrgMapping successfully deleted.";
                }
                else if (apiResponse != null && apiResponse.Data != null)
                {
                    ApiResponse<string> data = JsonConvert.DeserializeObject<ApiResponse<string>>(apiResponse.Data);
                    return $"Error Failed to delete customers. Error: {data.ErrorMessage}";
                }
                else
                {
                    return "An unexpected error occurred.";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<List<IListItem>> GetAuthTypeDDDataFromAPIAsync()
        {
            try
            {

                var listItemRequest = new ListItemRequest
                {
                    Codes = new List<string> { "AuthType" },
                    isCountRequired = true
                };
                if (listItemRequest != null)
                {
                    ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}ListItemHeader/GetListItemsByCodes",
                    HttpMethod.Post, listItemRequest);
                    if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                    {
                        ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>>>(apiResponse.Data);
                        if (pagedResponse != null)
                        {
                            return pagedResponse.Data.PagedData.OfType<IListItem>().ToList();
                        }
                    }
                }

            }
            catch (Exception)
            {

            }
            return null;
        }
        private async Task<List<IListItem>> GetDepartmentDDDataFromAPIAsync()
        {
            try
            {

                var listItemRequest = new ListItemRequest
                {
                    Codes = new List<string> { "DEPARTMENT" },
                    isCountRequired = true
                };
                if (listItemRequest != null)
                {
                    ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}ListItemHeader/GetListItemsByCodes",
                    HttpMethod.Post, listItemRequest);
                    if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                    {
                        ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>>>(apiResponse.Data);
                        if (pagedResponse != null)
                        {
                            return pagedResponse.Data.PagedData.OfType<IListItem>().ToList();
                        }
                    }
                }

            }
            catch (Exception)
            {

            }
            return null;
        }
        //private async Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>> GetOrgDDDataFromAPIAsync()
        //{
        //    try
        //    {
        //        PagingRequest pagingRequest = new PagingRequest();
        //        pagingRequest.PageNumber = 1;
        //        pagingRequest.PageSize = int.MaxValue;
        //        pagingRequest.FilterCriterias = new List<FilterCriteria>();
        //        pagingRequest.IsCountRequired = true;
        //        //pagingRequest.FilterCriterias.Add(new FilterCriteria("UID", "FRWH", FilterType.Equal));
        //        ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
        //            $"{_appConfigs.ApiBaseUrl}Org/GetOrgDetails",
        //            HttpMethod.Post, pagingRequest);
        //        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
        //        {
        //            string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
        //            PagedResponse<Winit.Modules.Org.Model.Classes.Org> selectionORGs = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Org.Model.Classes.Org>>(data);
        //            if (selectionORGs.PagedData != null)
        //            {
        //                return selectionORGs.PagedData.OfType<IOrg>().ToList();
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return null;
        //}

        private async Task<List<Winit.Modules.Role.Model.Interfaces.IRole>> GetRoleDDDataFromAPIAsync(string? roleuid)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias = new List<FilterCriteria>();
                // pagingRequest.FilterCriterias.Add(new FilterCriteria("OrgTypeUID", "Supplier", FilterType.Equal));

                ApiResponse<List<Winit.Modules.Role.Model.Classes.Role>> apiResponse =
                   await _apiService.FetchDataAsync<List<Winit.Modules.Role.Model.Classes.Role>>(
                   $"{_appConfigs.ApiBaseUrl}Role/SelectRolesByOrgUID?orguid={roleuid}",
                   HttpMethod.Get);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.OfType<IRole>().ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }
        private async Task<List<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetReportsToDDDataFromAPIAsync(string? empuid)
        {
            try
            {
                ApiResponse<List<Winit.Modules.Emp.Model.Classes.Emp>> apiResponse =
                    await _apiService.FetchDataAsync<List<Winit.Modules.Emp.Model.Classes.Emp>>(
                    $"{_appConfigs.ApiBaseUrl}Emp/GetReportsToEmployeesByRoleUID?roleUID={empuid}",
                    HttpMethod.Get);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.OfType<IEmp>().ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        private async Task<bool> CreateUpdateOrg_RoleMappingFromAPIAsync(Winit.Modules.JobPosition.Model.Interfaces.IJobPosition jobPosition, bool IsCreate)
        {
            try
            {
                ApiResponse<string> apiResponse = null;
                if (IsCreate)
                {
                    string jsonBody = JsonConvert.SerializeObject(jobPosition);
                    apiResponse = await _apiService.FetchDataAsync(
              $"{_appConfigs.ApiBaseUrl}JobPosition/CreateJobPosition", HttpMethod.Post, jobPosition);
                }
                else
                {
                    if (_appuser.Role.Code == "Sales Administration")
                    {
                        ApprovalRequestItem approvalRequestItem = PrepareApprovalRequestItem();
                        Winit.Modules.JobPosition.Model.Classes.JobPositionApprovalDTO jobPositionApprovalDTO = new JobPosition.Model.Classes.JobPositionApprovalDTO();
                        jobPositionApprovalDTO.ApprovalRequestItem = approvalRequestItem;
                        jobPositionApprovalDTO.JobPosition = jobPosition;
                        string jsonBody = JsonConvert.SerializeObject(jobPosition);
                        apiResponse = await _apiService.FetchDataAsync(
                                    $"{_appConfigs.ApiBaseUrl}JobPosition/UpdateJobPosition1", HttpMethod.Put, jobPositionApprovalDTO);
                    }
                    else
                    {
                        string jsonBody = JsonConvert.SerializeObject(jobPosition);
                        apiResponse = await _apiService.FetchDataAsync(
                                    $"{_appConfigs.ApiBaseUrl}JobPosition/UpdateJobPosition", HttpMethod.Put, jobPosition);
                    }

                }

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    if (apiResponse?.IsSuccess != null && apiResponse.IsSuccess)
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
        private ApprovalRequestItem? PrepareApprovalRequestItem()
        {
            ApprovalRequestItem approvalRequestItem = new ApprovalRequestItem();
            approvalRequestItem.HierarchyType = UserHierarchyTypeConst.Emp;
            approvalRequestItem.HierarchyUid = _appuser?.Emp?.UID!;
            approvalRequestItem.RuleId = GetRuleIdByName(ApprovalParameterConstants.RuleNameForMaintainUser);
            var customer = new ApprovalEngine.Model.Classes.Customer
            {
                ClassificationType = ApprovalParameterConstants.RuleParamForMaintainUser
            };

            // Payload dictionary initialize karna
            approvalRequestItem.Payload = new Dictionary<string, object>
            {
                { "RequesterId", _appuser?.Emp?.Code ?? "" },
                { "UserRoleCode", _appuser?.Role?.Code ?? "" },
                { "Remarks", "Need approval" }
            };

            // Alag se Customer key add karna
            approvalRequestItem.Payload.Add("Customer", customer);

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
        private async Task<bool> CreateUpdateUserFromAPIAsync(Winit.Modules.User.Model.Interfaces.IEmpDTO user, bool IsCreate)
        {
            try
            {
                ApiResponse<string> apiResponse = null;
                if (IsCreate)
                {
                    string jsonBody = JsonConvert.SerializeObject(user);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}MaintainUser/CUDEmployee", HttpMethod.Post, user);
                }
                else
                {
                    string jsonBody = JsonConvert.SerializeObject(user);
                    apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}MaintainUser/CUDEmployee", HttpMethod.Post, user);
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


        private async Task<List<IOrg>> GetEmpOrgMappingToDDDataFromAPIAsync(string orgUID)
        {
            try
            {
                ApiResponse<List<Winit.Modules.Org.Model.Classes.Org>> apiResponse =
                    await _apiService.FetchDataAsync<List<Winit.Modules.Org.Model.Classes.Org>>(
                    $"{_appConfigs.ApiBaseUrl}MaintainUser/GetAplicableOrgs?orgUID={orgUID}",
                    HttpMethod.Post);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.OfType<IOrg>().ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        private async Task<bool> CreateUpdateEmpOrgMappingFromAPIAsync(List<EmpOrgMapping> empOrgMapping)
        {
            try
            {
                ApiResponse<string> apiResponse = null;
                string jsonBody = JsonConvert.SerializeObject(empOrgMapping);
                apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}EmpOrgMapping/CreateEmpOrgMapping", HttpMethod.Post, empOrgMapping);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    //string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override async Task<string> CreateUpdateFileSysDetails(Winit.Modules.FileSys.Model.Interfaces.IFileSys fileSys)
        {
            return await SaveFileSysDataFromAPIAsync(fileSys);
        }
        private async Task<string> SaveFileSysDataFromAPIAsync(Winit.Modules.FileSys.Model.Interfaces.IFileSys fileSys)
        {
            string data = string.Empty;
            try
            {
                ApiResponse<string> apiResponse = null;

                apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}FileSys/CUDFileSys", HttpMethod.Post, fileSys);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return data;
        }
        #region Location and SKU Mapping

        public async Task GetLocationAndSKUMappingDetailFromAPI()
        {
            await GetSKUMappingdata();
            await GetLocationMappingdata();
        }


        public async Task GetLocationMappingdata()
        {
            LocationMappingList = new();
            try
            {
                ApiResponse<List<LocationTemplate>> apiResponse =
                    await _apiService.FetchDataAsync<List<LocationTemplate>>(
                    $"{_appConfigs.ApiBaseUrl}LocationTemplate/SelectAllLocationTemplates",
                    HttpMethod.Post);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    LocationTemplatesCopy = apiResponse.Data;
                    ConvertLocationMappingDataintoSelectionItem();
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task GetSKUMappingdata()
        {
            PagingRequest pagingRequest = new PagingRequest();
            SKUMappingList = new();
            try
            {
                ApiResponse<string> apiResponse =
                    await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}SKUTemplate/SelectAllSKUTemplateDetails",
                    HttpMethod.Post, pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    PagedResponse<SKUTemplate>? pagedResponse = JsonConvert.DeserializeObject<PagedResponse<SKUTemplate>>(new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data));
                    SKUTemplatesCopy = pagedResponse?.PagedData?.ToList();
                    ConvertSKUMappingDataintoSelectionItem();
                }
            }
            catch (Exception ex)
            {

            }
        }
        IJobPosition JobPosition { get; set; } = new JobPosition.Model.Classes.JobPosition();
        public async Task<IJobPosition> GetJobPositionByEmpUID()
        {
            ApiResponse<Winit.Modules.JobPosition.Model.Classes.JobPosition> apiResponse =
                   await _apiService.FetchDataAsync<Winit.Modules.JobPosition.Model.Classes.JobPosition>(
                   $"{_appConfigs.ApiBaseUrl}JobPosition/SelectJobPositionByEmpUID?EmpUID={EmpDTOmaintainUser?.Emp?.UID}",
                   HttpMethod.Get);
            if (apiResponse != null)
            {
                JobPosition = apiResponse.Data;
            }
            return JobPosition;

        }
        public async Task SaveLocationAndSKUMapping()
        {
            try
            {
                JobPosition.LocationMappingTemplateUID = SelectedLocationMapping?.UID;
                JobPosition.SKUMappingTemplateUID = SelectedSKUMapping?.UID;

                ApiResponse<string> apiResponse =
                    await _apiService.FetchDataAsync<string>(
                    $"{_appConfigs.ApiBaseUrl}JobPosition/UpdateJobPosition",
                    HttpMethod.Put, JobPosition);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    _toast.Add("Success", "Saved Successfully", Severity.Success);
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region Branch & SalesOffice
        public override async Task GetAllTheBranches()
        {
            List<IBranch> branches = await GetAllBranchesFromAPI();
            BranchSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems<IBranch>(branches, new List<string> { "UID", "Code", "Name" }));
        }

        public override async Task GetSalesOfficesByBranchUID(string BranchUID)
        {
            List<ISalesOffice> salesOffices = await GetSalesOfficesByBranchUIDFromAPI(BranchUID);
            SalesOfficeSelectionItems.Clear();
            SalesOfficeSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems<ISalesOffice>(salesOffices, new List<string> { "UID", "Code", "Name" }));
        }
        public async Task<List<Winit.Modules.Location.Model.Interfaces.IBranch>> GetAllBranchesFromAPI()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                ApiResponse<PagedResponse<IBranch>> apiResponse =
                   await _apiService.FetchDataAsync<PagedResponse<IBranch>>(
                   $"{_appConfigs.ApiBaseUrl}Branch/SelectAllBranchDetails",
                   HttpMethod.Post, pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
                {
                    return apiResponse.Data.PagedData.ToList<IBranch>();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return [];

        }
        private async Task<List<ISalesOffice>> GetSalesOfficesByBranchUIDFromAPI(string BranchUID)
        {
            try
            {
                var encodedUID = Uri.EscapeDataString(BranchUID);

                ApiResponse<List<ISalesOffice>> apiResponse =
                   await _apiService.FetchDataAsync<List<ISalesOffice>>(
                   $"{_appConfigs.ApiBaseUrl}SalesOffice/GetSalesOfficeByUID?UID={encodedUID}",
                   HttpMethod.Get);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToList();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }

        public override async Task<IOrg> GetOrgDetailsByUID(string OrgUID)
        {
            try
            {
                ApiResponse<Winit.Modules.Org.Model.Classes.Org> apiResponse =
                    await _apiService.FetchDataAsync<Winit.Modules.Org.Model.Classes.Org>(
                    $"{_appConfigs.ApiBaseUrl}Org/GetOrgByUID?UID={OrgUID}",
                    HttpMethod.Get);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }


        #endregion
    }
}
