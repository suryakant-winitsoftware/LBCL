using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinIT.RuleEngine.UI.Models
{
    [Serializable]
    public class RuleViewModel
    {
        public RuleViewModel()
        {
            ruleMaster = new RuleMaster();
            ruleParameters = new List<RuleParameter>();
            ruleConditions = new List<RuleCondition>();
            ruleActions = new List<RuleAction>();
            approvalHierarchies = new List<ApprovalHierarchy>();
        }
        public RuleMaster ruleMaster { get; set; }
        public RuleParameter editRuleParameter { get; set; }
        public RuleCondition editRuleCondition { get; set; }
        public RuleAction editRuleAction { get; set; }
        public ApprovalHierarchy editApprovalHierarchy { get; set; }

        public IList<RuleParameter> ruleParameters { get; set; }
        public IList<RuleCondition> ruleConditions { get; set; }
        public IList<RuleAction> ruleActions { get; set; }
        public IList<ApprovalHierarchy> approvalHierarchies { get; set; }
    }
}
