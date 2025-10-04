using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.User.Model.Interface;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
namespace Winit.Modules.ApprovalEngine.BL.Interfaces
{
    public interface IApprovalEngineBL
    {
        Task<IEnumerable<Model.Interfaces.IApprovalLog>> GetApprovalLog(string requestId);
        Task<List<ISelectionItem>> GetRoleNames();
        Task<IEnumerable<IApprovalHierarchy>> GetApprovalHierarchy(string ruleId);
        Task<IEnumerable<IViewChangeRequestApproval>> GetAllChangeRequest();
        Task<IAllApprovalRequest> GetApprovalDetailsByLinkedItemUid(string requestUid);
        Task<IViewChangeRequestApproval> GetChangeRequestDataByUid(string requestUid);
        Task<int> UpdateChangesInMainTable(IViewChangeRequestApproval? viewChangeRequestApproval);
        Task<List<IApprovalRuleMaster>> GetApprovalRuleMasterData();
        Task<List<IUserHierarchy>> GetUserHierarchyForRule(string hierarchyType, string hierarchyUID, int ruleId);
        Task<List<Winit.Modules.ApprovalEngine.Model.Interfaces.IApprovalRuleMap>> DropDownsForApprovalMapping();
        Task<int> IntegrateCreatedRule(IApprovalRuleMapping approvalRuleMapping);
        Task<int> GetRuleId(string type, string typeCode);
        Task<int> DeleteApprovalRequest(string requestId);
        Task<PagedResponse<IViewChangeRequestApproval>> GetChangeRequestData(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);

    }
}
