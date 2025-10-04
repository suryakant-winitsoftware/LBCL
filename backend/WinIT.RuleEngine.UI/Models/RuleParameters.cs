using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace WinIT.RuleEngine.UI.Models
{
    [Serializable]
    public class RuleParameter
    {
        public string Id { get; set; }
        public string RuleId { get; set; }
        [Required(ErrorMessage = "Enter Parameter Name")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Choose Data type")]
        public string DataType { get; set; }
        public string Description { get; set; }
    }

}
