using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models.RuleEngine
{
    public class RuleAction
    {
        public int Id { get; set; }
        public int RuleId { get; set; }
        public int ConditionId { get; set; }
        public string ActionType { get; set; }
        public string Template { get; set; }
    }
}
