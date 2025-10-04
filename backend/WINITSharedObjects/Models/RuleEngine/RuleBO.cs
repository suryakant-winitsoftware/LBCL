using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models.RuleEngine
{
    public class RuleBO
    {
        public RuleBO()
        {
            Rule = new Rule();
            Conditions = new List<Condition>();
            ApprovalHierarchy = new ApprovalHierarchy();
            Actions = new List<Action>();
        }
        public Rule Rule { get; set; }
        public IList<Condition> Conditions { get; set; }
        public ApprovalHierarchy ApprovalHierarchy { get; set; }
        public IList<Action> Actions { get; set; }
    }
}
