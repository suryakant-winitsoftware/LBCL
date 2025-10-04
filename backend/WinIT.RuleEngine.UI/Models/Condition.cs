using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace WinIT.RuleEngine.UI.Models
{
    [Serializable]
    public class RuleCondition
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "Choose Operator")]
        public string Operator { get; set; }
        public string ParameterId { get; set; }
        [Required(ErrorMessage = "Enter Value")]
        public string Value { get; set; }
        public bool IsGroup { get; set; }
        public int? ParentConditionId { get; set; }
        public string RuleId { get; set; }
        [Required(ErrorMessage = "Choose Parameter")]
        public string ParameterName { get; set; }
        public string DataType { get; set; }


        
    }
   
}
