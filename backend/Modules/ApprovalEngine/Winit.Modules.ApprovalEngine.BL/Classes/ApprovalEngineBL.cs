using Winit.Modules.ApprovalEngine.BL.Interfaces;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.User.Model.Interface;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ApprovalEngine.BL.Classes
{
    public class ApprovalEngineBL : IApprovalEngineBL
    {
        DL.Interfaces.IApprovalEngineDL _IApprovalEngineDL;
        public ApprovalEngineBL(DL.Interfaces.IApprovalEngineDL IApprovalEngineDL)
        {
            _IApprovalEngineDL=IApprovalEngineDL;
        }

        public async Task<IEnumerable<Model.Interfaces.IApprovalLog>> GetApprovalLog(string requestId)
        {
            return await _IApprovalEngineDL.GetApprovalLog(requestId);
        }
        public async Task<List<ISelectionItem>> GetRoleNames()
        {
            return await _IApprovalEngineDL.GetRoleNames();
        }
        public async Task<IEnumerable<IApprovalHierarchy>> GetApprovalHierarchy(string ruleId)
        {
            return await _IApprovalEngineDL.GetApprovalHierarchy(ruleId);
        }
        public async Task<IEnumerable<IViewChangeRequestApproval>> GetAllChangeRequest()
        {
            return await _IApprovalEngineDL.GetAllChangeRequest();
        }
        public async Task<IAllApprovalRequest> GetApprovalDetailsByLinkedItemUid(string linkItemUID)
        {
            return await _IApprovalEngineDL.GetApprovalDetailsByLinkedItemUid(linkItemUID);
        }
        public async Task<IViewChangeRequestApproval> GetChangeRequestDataByUid(string requestUid)
        {
            return await _IApprovalEngineDL.GetChangeRequestDataByUid(requestUid);
        }
        public async Task<int> UpdateChangesInMainTable(IViewChangeRequestApproval? viewChangeRequestApproval)
        {
            return await _IApprovalEngineDL.UpdateChangesInMainTable(viewChangeRequestApproval);
        }
        public async Task<List<IApprovalRuleMaster>> GetApprovalRuleMasterData()
        {
            return await _IApprovalEngineDL.GetApprovalRuleMasterData();
        }
        public async Task<List<IUserHierarchy>> GetUserHierarchyForRule(string hierarchyType, string hierarchyUID, int ruleId)
        {
            return await _IApprovalEngineDL.GetUserHierarchyForRule(hierarchyType, hierarchyUID, ruleId);
        }
        public async Task<List<Winit.Modules.ApprovalEngine.Model.Interfaces.IApprovalRuleMap>> DropDownsForApprovalMapping()
        {
            return await _IApprovalEngineDL.DropDownsForApprovalMapping();
        }
        public async Task<int> IntegrateCreatedRule(IApprovalRuleMapping approvalRuleMapping)
        {
            return await _IApprovalEngineDL.IntegrateCreatedRule(approvalRuleMapping);
        }
        public async Task<int> GetRuleId(string type, string typeCode)
        {
            return await _IApprovalEngineDL.GetRuleId(type, typeCode);
        }
        public async Task<int> DeleteApprovalRequest(string requestId)
        {
            return await _IApprovalEngineDL.DeleteApprovalRequest(requestId);
        }
        public async Task<PagedResponse<IViewChangeRequestApproval>> GetChangeRequestData(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _IApprovalEngineDL.GetChangeRequestData(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
    }
}
