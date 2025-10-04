using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Models.RuleEngine;

namespace WINITRepository.Interfaces.RuleEngine
{
    public interface IRuleEngineRepository
    {
        Task<Rule> RetrieveRuleAsync(int ruleId);
        Task<List<Condition>> RetrieveConditionsAsync(int ruleId);
        Task<List<WINITSharedObjects.Models.RuleEngine.Action>> RetrieveActionsAsync(int ruleId);
        Task<int> InsertApprovalRequestAsync(ApprovalRequest approvalRequest);
        Task<int> UpdateApprovalRequestStatusAsync(int requestId, int ruleId, string status, string modifyBy);
        Task<int> InsertApprovalStatusAsync(ApprovalStatus approvalStatus);
        Task<int> UpdateApprovalStatusAsync(ApprovalStatus approvalStatus);
        Task<List<ApprovalHierarchy>> RetrieveApprovalHierarchyAsync(int ruleId);
        Task<int> InsertApproverAsync(int approvalRequestId, string ApproverId);
        Task<int> InsertApprovalRequestActionAsync(int approvalRequestId, long ActionId);
        Task<List<WINITSharedObjects.Models.RuleEngine.Action>> RetrieveActionsByRequestIdAsync(int requestId);
        Task<List<ApprovalHierarchy>> RetrieveApprovalHierarchyByRequestIdAsync(int requestId);
        Task<Users> RetrieveRequesterDetailsByRequestIdAsync(int requestId);

        Task<int> InsertRuleMaster(RuleMaster ruleMaster);
        Task<int> UpsertRuleParameter(RuleParameter ruleParameter);
        Task<int> UpsertCondition(Condition condition);
        Task<int> UpsertRuleAction(RuleAction ruleAction);
        Task<int> UpsertApprovalHierarchy(ApprovalHierarchy approvalHierarchy);
        Task<List<KeyValueObject<string, string>>> RetrieveApproverAllAsync();
        Task<List<Rule>> RetrieveAllRuleAsync();
    }
}
