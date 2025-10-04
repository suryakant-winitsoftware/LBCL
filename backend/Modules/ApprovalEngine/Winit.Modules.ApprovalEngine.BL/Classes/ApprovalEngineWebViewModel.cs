using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Constants;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common;
namespace Winit.Modules.ApprovalEngine.BL.Classes
{
    public class ApprovalEngineWebViewModel : ApprovalEngineBaseViewModel
    {
        private readonly ApiSettings _apiSettings;
        private readonly IConfiguration _configuration;
        public readonly string ApprovalApiURL;
        public ApprovalEngineWebViewModel(IConfiguration configuration, IServiceProvider serviceProvider,
         IAppUser appUser,
         Shared.Models.Common.IAppConfig appConfigs,
         Base.BL.ApiService apiService, IOptions<ApiSettings> apiSettings)
         : base(serviceProvider, appUser, appConfigs, apiService)
        {
            _configuration = configuration;
            _apiSettings = apiSettings.Value;
            ApprovalApiURL = _configuration["approvalEngineApiUrl"];
            // ApprovalApiURL = "https://localhost:5004/api/RuleEngine/";
        }

        public override async Task<bool> SendNotificationToNextApprover(List<string> listOfNextApprovers)
        {
            try
            {
                Notification notification = new Notification();
                notification.NextApprovers = listOfNextApprovers;
                notification.NotificationMessage="A new form is awaiting your approval";
                ApiResponse<string> apiResponse =
               await _apiService.FetchDataAsync(
               $"{_appConfigs.ApiBaseUrl}ApprovalEngine/NotifiedToNextApprovers",
               HttpMethod.Post, notification);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return true;
                }
                else { return false; }

            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public override async Task DropDownsForApprovalMapping()
        {
            try
            {
                ApiResponse<List<ApprovalRuleMap>> apiResponse =
                 await _apiService.FetchDataAsync<List<ApprovalRuleMap>>(
                 $"{_appConfigs.ApiBaseUrl}ApprovalEngine/DropDownsForApprovalMapping",
                 HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    RuleMap = apiResponse.Data.Cast<IApprovalRuleMap>().ToList();
                    Type = CommonFunctions.ConvertToSelectionItems(
                        RuleMap
                            .GroupBy(rule => rule.Type)  // Group by UID to ensure distinctness
                            .Select(group => group.First())  // Select the first rule from each group
                            .ToList(),
                        new List<string> { "Type", "Type", "Type" });
                }

            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public override async Task<bool> IntegrateRule()
        {
            try
            {
                PrepareApprovalRuleMap();
                ApiResponse<string> apiResponse =
                 await _apiService.FetchDataAsync(
                 $"{_appConfigs.ApiBaseUrl}ApprovalEngine/IntegrateRule",
                 HttpMethod.Post, ApprovalRuleMapping);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return true;
                }
                else { return false; }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            finally
            {

            }
        }
        public override void PrepareApprovalRuleMap()
        {
            ApprovalRuleMapping.UID=Guid.NewGuid().ToString();
            ApprovalRuleMapping.ModifiedBy=_appUser.Emp.ModifiedBy;
            ApprovalRuleMapping.CreatedBy=_appUser.Emp.CreatedBy;
            ApprovalRuleMapping.CreatedTime=DateTime.Now;
            ApprovalRuleMapping.ModifiedTime=DateTime.Now;
        }

        public override async Task GetAllChangeRequestDataAsync()
        {
            //ApiResponse<List<ViewChangeRequestApproval>> apiResponse = await _apiService.FetchDataAsync<List<ViewChangeRequestApproval>>($"{_appConfigs.ApiBaseUrl}ApprovalEngine/GetAllChangeRequest", HttpMethod.Get);
            //if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            //{
            //    ViewChangeRequestApprovals = apiResponse.Data.ToList<IViewChangeRequestApproval>();
            //}
            try
            {
                PagingRequest.PageNumber = PageNumber;
                PagingRequest.PageSize = PageSize;
                PagingRequest.IsCountRequired = true;
                PagingRequest.SortCriterias = new List<SortCriteria>
                {
                    new SortCriteria("ModifiedTime",SortDirection.Desc)
                };
                PagingRequest.SortCriterias = this.SortCriterias;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}ApprovalEngine/GetChangeRequestData",
                    HttpMethod.Post, PagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<ViewChangeRequestApproval>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<ViewChangeRequestApproval>>>(apiResponse.Data);
                    TotalChangeRequests = pagedResponse.Data.TotalCount;
                    ViewChangeRequestApprovals= pagedResponse.Data.PagedData.ToList<IViewChangeRequestApproval>();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public override async Task GetChangeRequestDataByUIDAsync(string UID)
        {
            ApiResponse<ViewChangeRequestApproval> apiResponse = await _apiService.FetchDataAsync<ViewChangeRequestApproval>($"{_appConfigs.ApiBaseUrl}ApprovalEngine/GetChangeRequestDataByUid?requestUid={UID}", HttpMethod.Get);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                DisplayChangeRequestApproval = (IViewChangeRequestApproval)apiResponse.Data;
                ChangeRecordDTOs = JsonConvert.DeserializeObject<List<ChangeRecordDTO>>(DisplayChangeRequestApproval.ChangedRecord)!;
            }
        }
        public override async Task GetRequestId(string UID)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<Winit.Modules.ApprovalEngine.Model.Classes.AllApprovalRequest> AllApprovalLevelDetails = await
                    _apiService.FetchDataAsync<Winit.Modules.ApprovalEngine.Model.Classes.AllApprovalRequest>
                    ($"{_appConfigs.ApiBaseUrl}ApprovalEngine/GetApprovalDetailsByLinkedItemUid?requestUid={UID}", HttpMethod.Get);
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
        public override async Task<bool> UpdateApprovalStatus(ApprovalStatusUpdate approvalStatusUpdate)
        {
            var payload = new
            {
                status = approvalStatusUpdate.Status,
                comment = approvalStatusUpdate.Remarks,
                requesterid = approvalStatusUpdate.RequesterId,
                roleid = approvalStatusUpdate.RoleCode
            };
            var response = await _apiService.FetchDataAsync<ApprovalApiResponse<dynamic>>(_configuration["approvalEngineApiUrl"] + ApprovalConst.Action + approvalStatusUpdate.RequestId, HttpMethod.Post, payload);
            return response.IsSuccess;
        }
        public override async Task<ApiResponse<string>> UpdateChangesInMainTable()
        {
            try
            {
                ApiResponse<string>? apiResponse = null;
                apiResponse = await
                   _apiService.FetchDataAsync
                   ($"{_appConfigs.ApiBaseUrl}ApprovalEngine/UpdateChangesInMainTable", HttpMethod.Put, DisplayChangeRequestApproval);
                Console.WriteLine(CommonFunctions.ConvertToJson(apiResponse));
                return apiResponse;
            }

            catch (Exception)
            {
                throw;
            }
        }

        #region Approval Delete Logic

        public override async Task<bool> DeleteAllApprovalRequest(string requestId)
        {
            try
            {


                ApiResponse<string>? apiResponse = null;

                apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}ApprovalEngine/DeleteApprovalRequest?requestId={requestId}", HttpMethod.Delete);
                if (apiResponse != null)
                {
                    return true;
                }
                else { return false; }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            finally
            {

            }
        }
        #endregion

        #region FetchHierarchy
        public override async Task FetchApprovalHierarchyStatus(string requestId)
        {
            try
            {
                ApiResponse<ApprovalStatus> apiResponse = await _apiService.FetchDataAsync<ApprovalStatus>(ApprovalApiURL
                 + ApprovalConst.GetApprovalHierarchyStatus + requestId, HttpMethod.Get);
                
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApprovalStatus approvalHierarchyData = apiResponse.Data;
                    
                    if (approvalHierarchyData.approvalStatus != null)
                    {
                        var filteredApprovalStatus = approvalHierarchyData.approvalStatus?
                            .Where(a => ApprovalRoleCodes.Contains(a.ApproverId))
                            .ToList();
                        var filteredApprovalLogs = approvalHierarchyData.approvalLog?
                           .Where(a => ApprovalRoleCodes.Contains(a.ApproverId))
                           .ToList();
                        if (filteredApprovalStatus != null)
                        {
                            for (int i = 0; i < filteredApprovalStatus.Count; i++)
                            {
                                filteredApprovalStatus[i].ApproverLevel = i + 1; // Reassign levels sequentially
                            }
                        }
                        if (filteredApprovalLogs != null)
                        {
                            // Dictionary to store the level for each ApproverId (string type)
                            Dictionary<string, int> approverLevels = new Dictionary<string, int>();

                            int currentLevel = 1;

                            for (int i = 0; i < filteredApprovalLogs.Count; i++)
                            {
                                var approverId = filteredApprovalLogs[i].ApproverId;

                                // Check if the approverId has been assigned a level before
                                if (!approverLevels.ContainsKey(approverId))
                                {
                                    // If not, assign the current level and increment the level counter
                                    approverLevels[approverId] = currentLevel;
                                    currentLevel++;
                                }

                                // Assign the level for the current log
                                filteredApprovalLogs[i].Level = approverLevels[approverId];
                            }
                        }


                        ApprovalHierarchyData = new ApprovalStatus
                        {
                            RequestId = approvalHierarchyData.RequestId,
                            approvalStatus =filteredApprovalStatus,
                            approvalLog = filteredApprovalLogs
                        };
                        ReAssignOptions = ApprovalHierarchyData.approvalStatus.Where(i => (i.ApproverId != _appUser.Emp.Code && i.ApproverType == ApprovalConst.UserApprover) ||
                            (i.ApproverId != _appUser.Role.Code && i.ApproverType == ApprovalConst.RoleApprover)).Select(it => it.ApproverLevel).ToList();
                        var userLevel = ApprovalHierarchyData.approvalStatus.Where(i => (i.ApproverId == _appUser.Emp.Code && i.ApproverType == ApprovalConst.UserApprover) ||
                            (i.ApproverId == _appUser.Role.Code && i.ApproverType == ApprovalConst.RoleApprover)).Select(it => it.ApproverLevel).ToList();
                        foreach (var item in ApprovalHierarchyData.approvalStatus)
                        {
                            if (!UserLevelRoleMap.ContainsKey(item.ApproverLevel))
                            {
                                UserLevelRoleMap.Add(item.ApproverLevel, item.RoleName);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Approval hierarchy not available - approval status is null");
                    }
                }
                else
                {
                    throw new Exception("Something went wrong or Approval hierarchy not available");
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        #endregion

        #region ReAssign
        public override async Task<bool> Reassign(string selectedOption, string remark, int approverLevel, string requestId, string approverType)
        {

            try
            {

                var payload = new ApprovalLogs
                {
                    ApproverId = (approverType == ApprovalConst.RoleApprover ? _appUser.Role.Code : _appUser.Emp.Code),
                    Comments = remark,
                    level = approverLevel,
                    modifiedBy = _appUser.Emp.Code,
                    reassignTo = selectedOption,
                    requestId = Convert.ToInt64(requestId),
                    Status = ApprovalConst.SendingReassign
                };
                var apiResponse = await _apiService.FetchDataAsync<ApprovalApiResponse<dynamic>>(ApprovalApiURL + ApprovalConst.ReassignAction + requestId, HttpMethod.Post, payload);
                
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    var approvalResponse = apiResponse.Data;
                    if (approvalResponse.Success == true)
                    {
                        await FetchApprovalHierarchyStatus(requestId);
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion
        public override async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {

            PagingRequest.FilterCriterias!.Clear();
            if (filterCriteria is not null)
            {

                foreach (var item in filterCriteria)
                {
                    if (!string.IsNullOrEmpty(item.Value))
                    {
                        switch (item.Key)
                        {
                            case nameof(IViewChangeRequestApproval.RequestDate):
                                PagingRequest?.FilterCriterias.Add(
                                new FilterCriteria(nameof(IViewChangeRequestApproval.RequestDate), CommonFunctions.GetDate(item.Value),
                               FilterType.GreaterThanOrEqual));
                                break;
                            case nameof(IViewChangeRequestApproval.ApprovedDate):
                                PagingRequest?.FilterCriterias.Add(
                                new FilterCriteria(nameof(IViewChangeRequestApproval.ApprovedDate), CommonFunctions.GetDate(item.Value),
                               FilterType.LessThanOrEqual));
                                break;
                            case nameof(IViewChangeRequestApproval.Status):
                                if (item.Value.Contains(","))
                                {
                                    string[] values = item.Value.Split(',');
                                    PagingRequest?.FilterCriterias.Add(new FilterCriteria(nameof(IViewChangeRequestApproval.Status), values, FilterType.In));
                                }
                                else
                                {
                                    PagingRequest?.FilterCriterias.Add(new FilterCriteria(nameof(IViewChangeRequestApproval.Status), item.Value, FilterType.Equal));
                                }
                                break;
                            default:
                                PagingRequest?.FilterCriterias.Add(new FilterCriteria(item.Key, item.Value, FilterType.Equal));
                                break;
                        }
                    }

                }

            }
            await GetAllChangeRequestDataAsync();
        }
        public async Task GetChangeRequestData()
        {
            try
            {
                PagingRequest.PageNumber = PageNumber;
                PagingRequest.PageSize = PageSize;
                PagingRequest.IsCountRequired = true;
                PagingRequest.SortCriterias = new List<SortCriteria>
                {
                    new SortCriteria("ModifiedTime",SortDirection.Desc)
                };
                PagingRequest.SortCriterias = this.SortCriterias;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}ApprovalEngine/GetChangeRequestData",
                    HttpMethod.Post, PagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<IViewChangeRequestApproval>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<IViewChangeRequestApproval>>>(apiResponse.Data);
                    //TotalChangeRequests = pagedResponse.Data.TotalCount;
                    ViewChangeRequestApprovals= pagedResponse.Data.PagedData.ToList<IViewChangeRequestApproval>();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public override async Task ApplySort(SortCriteria sortCriteria)
        {
            SortCriterias.Clear();
            SortCriterias.Add(sortCriteria);
            await GetAllChangeRequestDataAsync();
        }
        public override async Task PageIndexChanged(int pageNumber)
        {
            PageNumber=pageNumber;
            await GetAllChangeRequestDataAsync();
        }
    }
    public class Notification()
    {
        public List<string> NextApprovers = new List<string>();
        public string NotificationMessage { get; set; }
    }


}
