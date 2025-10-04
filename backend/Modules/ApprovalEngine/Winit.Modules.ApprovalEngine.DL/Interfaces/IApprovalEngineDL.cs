using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.User.Model.Interface;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ApprovalEngine.DL.Interfaces
{
    public interface IApprovalEngineDL
    {
        Task<IEnumerable<Model.Interfaces.IApprovalLog>> GetApprovalLog(string RequestId);
        Task<List<ISelectionItem>> GetRoleNames();
        Task<IEnumerable<IApprovalHierarchy>> GetApprovalHierarchy(string ruleId);
        Task<IEnumerable<IViewChangeRequestApproval>> GetAllChangeRequest();
        Task<IAllApprovalRequest?> GetApprovalDetailsByLinkedItemUid(string linkItemUID);
        Task<int> UpdateChangesInMainTable(IViewChangeRequestApproval? viewChangeRequestApproval);
        Task<List<IApprovalRuleMaster>> GetApprovalRuleMasterData();
        Task<List<IUserHierarchy>> GetUserHierarchyForRule(string hierarchyType, string hierarchyUID, int ruleId);
        Task<IViewChangeRequestApproval> GetChangeRequestDataByUid(string requestUid);
        Task<List<Winit.Modules.ApprovalEngine.Model.Interfaces.IApprovalRuleMap>> DropDownsForApprovalMapping();
        Task<int> IntegrateCreatedRule(IApprovalRuleMapping approRuleMap);
        Task<int> GetRuleId(string type, string typeCode);
        Task<int> DeleteApprovalRequest(string requestId);
        Task<int> CreateAllApprovalRequest(IAllApprovalRequest allApprovalRequest);
        Task<PagedResponse<IViewChangeRequestApproval>> GetChangeRequestData(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
    }
}
