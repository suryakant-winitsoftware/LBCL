using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models.RuleEngine
{
    public class Rule
    {
        public long Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        //public bool IsEnabled { get; private set; }
        //public IList<Condition> Conditions { get; set; }
        //public ApprovalHierarchy ApprovalHierarchy { get; set; }
        //public IList<Action> Actions { get; set; }
        public Rule()
        {

        }
        //public Rule(string name, string description, bool isEnabled, IList<Condition> conditions, ApprovalHierarchy approvalHierarchy, IList<Action> actions)
        //{
        //    Name = name;
        //    Description = description;
        //    IsEnabled = isEnabled;
        //    Conditions = conditions ?? throw new ArgumentNullException(nameof(conditions));
        //    ApprovalHierarchy = approvalHierarchy ?? throw new ArgumentNullException(nameof(approvalHierarchy));
        //    Actions = actions ?? throw new ArgumentNullException(nameof(actions));
        //}
        //public void Enable()
        //{
        //    IsEnabled = true;
        //}
        //public void Disable()
        //{
        //    IsEnabled = false;
        //}
    }
}
