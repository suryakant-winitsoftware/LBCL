using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models.RuleEngine
{
    public class ApprovalStatus
    {
        public int ApprovalRequestId { get; set; }
        public string ApproverId { get; set; }
        public string Status { get; set; }
        public long ActionId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Remarks { get; set; }
    }

}
