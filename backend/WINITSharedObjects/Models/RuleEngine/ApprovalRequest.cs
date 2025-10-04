using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models.RuleEngine
{
    public class ApprovalRequest
    {
        public long Id { get; set; }
        public int RuleId { get; set; }
        public string RequesterId { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }

        }
}
