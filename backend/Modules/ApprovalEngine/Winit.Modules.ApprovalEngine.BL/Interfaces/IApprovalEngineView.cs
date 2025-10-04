using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common;

namespace Winit.Modules.ApprovalEngine.BL.Interfaces
{
    public interface IApprovalEngineView
    {

        public int PageSize { get; set; }
        public int TotalChangeRequests { get; set; }
        public int PageNumber { get; set; }
        public Dictionary<int, string> UserLevelRoleMap { get; set; }
        // public bool CanReAssign { get; set; } 
        public ApprovalStatus? ApprovalHierarchyData { get; set; }
        public string ApiURL { get; set; }
        //  Task GetApprovalLog(string requestId);
        public List<string> ApprovalRoleCodes { get; set; }
        // Task GetRoleNames();
        public List<int> ReAssignOptions { get; set; }
        public List<ISelectionItem> RoleCode_NameKVP { get; set; }
        Task<bool> SendNotificationToNextApprover(List<string> listOfNextApprovers);
        Task DropDownsForApprovalMapping();
        public List<IViewChangeRequestApproval> ViewChangeRequestApprovals { get; set; }
        public IViewChangeRequestApproval DisplayChangeRequestApproval { get; set; }

        public List<ISelectionItem> Type { get; set; }
        public List<ISelectionItem> TypeCode { get; set; }
        public List<ISelectionItem> RuleIDs { get; set; }
        public List<IApprovalRuleMap> RuleMap { get; set; }
        public IApprovalRuleMapping ApprovalRuleMapping { get; set; }
        public Dictionary<string, List<EmployeeDetail>>? ApprovalUserCodes { get; set; }
        public IAllApprovalRequest AllApprovalRequestData { get; set; }
        public List<ChangeRecordDTO> ChangeRecordDTOs { get; set; }

        Task<bool> IntegrateRule();
        Task GetAllChangeRequestDataAsync();
        Task<bool> DeleteAllApprovalRequest(string requestId);
        Task GetRequestId(string uid);
        Task<ApiResponse<string>> UpdateChangesInMainTable();
        Task FetchApprovalHierarchyStatus(string requestId);
        Task GetChangeRequestDataByUIDAsync(string UID);
        Task<bool> UpdateApprovalStatus(ApprovalStatusUpdate approvalStatusUpdate);
        Task<bool> Reassign(string selectedOption, string remark, int approverLevel, string requestId, string approverType);
        Task OnFilterApply(Dictionary<string, string> filterCriteria);
        Task ApplySort(SortCriteria sortCriteria);
        Task PageIndexChanged(int pageNumber);
    }
}
