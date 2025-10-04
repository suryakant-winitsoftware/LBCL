using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITServices.Interfaces.RuleEngine;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Models.RuleEngine;

namespace WINITServices.Classes.RuleEngine
{
    public abstract class RuleEngineBaseService: IRuleEngineService
    {
        protected readonly WINITRepository.Interfaces.RuleEngine.IRuleEngineRepository _ruleEngineRepository;
        public RuleEngineBaseService(WINITRepository.Interfaces.RuleEngine.IRuleEngineRepository ruleEngineRepository)
        {
            _ruleEngineRepository = ruleEngineRepository;
        }
        public abstract Task<int> ApproveRejectRequest(int requestid, string status);
        public abstract Task<int> CreateRequest(int ruleId, Dictionary<string, object> parameters);
        public abstract Task<RuleBO> GetRule(int ruleId);
        public abstract bool HandleMessageReceivedAsync(MessageData messageData);

        public abstract Task<int> InsertRuleMaster(RuleMaster ruleMaster);
        public abstract Task<int> UpsertRuleParameter(RuleParameter ruleParameter);
        public abstract Task<int> UpsertCondition(Condition condition);
        public abstract Task<int> UpsertRuleAction(RuleAction ruleAction);
        public abstract Task<int> UpsertApprovalHierarchy(ApprovalHierarchy approvalHierarchy);

        public abstract Task<List<KeyValueObject<string, string>>> RetrieveApproverAllAsync();

        public abstract Task<List<Rule>> RetrieveAllRuleAsync();
    }
}
