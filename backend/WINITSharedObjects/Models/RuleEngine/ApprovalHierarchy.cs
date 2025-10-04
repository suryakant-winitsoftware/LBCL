using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models.RuleEngine
{
    public class ApprovalHierarchy
    {
        public long id { get; set; }
        public int RuleId { get; set; }
        public int Level { get; set; }
        public string ApproverId { get; set; }
        public string NextApproverId { get; set; }
        public string Email { get; set; }

    }
}
