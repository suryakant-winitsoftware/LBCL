using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Winit.Modules.ApprovalEngine.BL.Interfaces;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Constants;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Base.BL;

//using Winit.Modules.Store.BL.Interfaces;
//using Winit.Modules.User.BL.Interfaces;
using Winit.Modules.User.Model.Interface;
using Winit.UIModels.Common;
namespace Winit.Modules.ApprovalEngine.BL.Classes;

/// <summary>
/// Provides helper methods for approval engine operations including request creation and status updates.
/// </summary>
public class ApprovalEngineHelper : IApprovalEngineHelper
{
    #region Private Fields

    private readonly ApiService _apiService;
    private readonly IConfiguration _configuration;
    //private readonly IStoreBL _storeBl;
    DL.Interfaces.IApprovalEngineDL _approvalEngineDL;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the ApprovalEngineHelper class.
    /// </summary>
    /// <param name="apiService">API service for external communication</param>
    /// <param name="configuration">Configuration settings</param>
    /// <param name="approvalEngineDL">Data layer for approval engine operations</param>
    public ApprovalEngineHelper(ApiService apiService, IConfiguration configuration, DL.Interfaces.IApprovalEngineDL approvalEngineDL)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _approvalEngineDL = approvalEngineDL ?? throw new ArgumentNullException(nameof(approvalEngineDL));
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Creates an approval request by calling the approval engine API and storing the request details.
    /// </summary>
    /// <param name="approvalRequestItem">Approval request item details</param>
    /// <param name="allApprovalRequest">All approval request data</param>
    /// <returns>Approval API response with approval status</returns>
    public async Task<ApprovalApiResponse<ApprovalStatus>> CreateApprovalRequest(ApprovalRequestItem approvalRequestItem, IAllApprovalRequest allApprovalRequest)
    {
        try
        {
            var apiResponse = await _apiService.FetchDataAsync<ApprovalApiResponse<ApprovalStatus>>(
                _configuration["approvalEngineApiUrl"] + ApprovalConst.EvaluateRule + approvalRequestItem.RuleId,
                HttpMethod.Post,
                approvalRequestItem.Payload
            );

            // Check if API call was successful
            if (apiResponse == null || apiResponse.StatusCode != 200)
            {
                throw new Exception($"API call failed with status: {apiResponse?.StatusCode}. Message: {apiResponse?.ErrorMessage}");
            }

            // Extract the actual approval response from the API wrapper
            var approvalResponse = apiResponse.Data;
            
            if (approvalResponse == null || approvalResponse.data == null)
            {
                throw new Exception("The API response is null or contains no approval data.");
            }

            if (approvalResponse.data.RequestId == null)
            {
                throw new Exception("RequestId not generated. Please check the rule parameter.");
            }

            // Get user hierarchy for the rule
            List<IUserHierarchy> userHierarchies = await _approvalEngineDL.GetUserHierarchyForRule(
                approvalRequestItem.HierarchyType,
                approvalRequestItem.HierarchyUid,
                approvalRequestItem.RuleId
            );

            // Create user role mappings
            var userRoleMappings = userHierarchies
                .GroupBy(e => e.RoleCode)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(e => new { e.EmpCode, e.EmpName })
                        .ToList()
                );

            // Store approval request details
            allApprovalRequest.ApprovalUserDetail = JsonConvert.SerializeObject(userRoleMappings);
            allApprovalRequest.RequestID = approvalResponse.data.RequestId.ToString();
            
            await _approvalEngineDL.CreateAllApprovalRequest(allApprovalRequest);
            
            return approvalResponse;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("An error occurred while processing the approval request.", ex);
        }
    }

    /// <summary>
    /// Updates the approval status for a specific request.
    /// </summary>
    /// <param name="approvalStatusUpdate">Approval status update details</param>
    /// <returns>True if update was successful, false otherwise</returns>
    public async Task<bool> UpdateApprovalStatus(ApprovalStatusUpdate approvalStatusUpdate)
    {
        try
        {
            var payload = new
            {
                status = approvalStatusUpdate.Status,
                comment = approvalStatusUpdate.Remarks ?? "remarks",
                requesterid = approvalStatusUpdate.RequesterId,
                roleid = approvalStatusUpdate.RoleCode
            };

            var apiResponse = await _apiService.FetchDataAsync<ApprovalApiResponse<dynamic>>(
                _configuration["approvalEngineApiUrl"] + ApprovalConst.Action + approvalStatusUpdate.RequestId, 
                HttpMethod.Post, 
                payload
            );

            // Check if API call was successful
            if (apiResponse == null || apiResponse.StatusCode != 200)
            {
                return false;
            }

            // Extract the actual approval response and check success
            var approvalResponse = apiResponse.Data;
            return approvalResponse?.Success == true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    #endregion
}
