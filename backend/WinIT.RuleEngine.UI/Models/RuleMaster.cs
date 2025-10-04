using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WinIT.RuleEngine.UI.Models
{
    [Serializable]
    public class RuleMaster
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "Enter Rule Name")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Enter Rule Description")]
        public string Description { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
    }

}
