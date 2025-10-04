using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace WinIT.RuleEngine.UI.Models
{
    [Serializable]
    public class ApprovalHierarchy
    {
        public string id { get; set; }
        public string RuleId { get; set; }
        [Required(ErrorMessage = "Enter Level")]
        public int Level { get; set; }
        [Required(ErrorMessage = "Choose Approver")]
        public string ApproverId { get; set; }
        public string NextApproverId { get; set; }
        public string Email { get; set; }

    }
}
