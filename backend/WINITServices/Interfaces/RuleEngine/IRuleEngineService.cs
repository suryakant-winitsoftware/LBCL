using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Models.RuleEngine;

namespace WINITServices.Interfaces.RuleEngine
{
    public interface IRuleEngineService
    {
        Task<int> CreateRequest(int ruleId, Dictionary<string, object> parameters);
        Task<RuleBO> GetRule(int ruleId);
        Task<int> ApproveRejectRequest(int requestid, string status);
        bool HandleMessageReceivedAsync(MessageData messageData);

        Task<int> InsertRuleMaster(RuleMaster ruleMaster);
        Task<int> UpsertRuleParameter(RuleParameter ruleParameter);
        Task<int> UpsertCondition(Condition condition);
        Task<int> UpsertRuleAction(RuleAction ruleAction);
        Task<int> UpsertApprovalHierarchy(ApprovalHierarchy approvalHierarchy);
        Task<List<KeyValueObject<string, string>>> RetrieveApproverAllAsync();
        Task<List<Rule>> RetrieveAllRuleAsync();
    }
}
