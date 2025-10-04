using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models.RuleEngine
{
    public class Condition
    {
        public long Id { get; set; }
        public string Operator { get; set; }
        public int ParameterId { get; set; }
        public string Value { get; set; }
        public bool IsGroup { get; set; }
        public int? ParentConditionId { get; set; }
        public int RuleId { get; set; }
        public string ParameterName { get; set; }
        public string DataType { get; set; }
        public Condition()
        {
            // Default constructor
        }
        public Condition(long id, string @operator, int parameterId, string value, bool isGroup, int? parentConditionId, int ruleId)
        {
            this.Id = id;
            this.Operator = @operator;
            this.ParameterId = parameterId;
            this.Value = value;
            this.IsGroup = isGroup;
            this.ParentConditionId = parentConditionId;
            this.RuleId = ruleId;
        }
        
    }
    public enum ConditionOperator
    {
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual
    }
}
