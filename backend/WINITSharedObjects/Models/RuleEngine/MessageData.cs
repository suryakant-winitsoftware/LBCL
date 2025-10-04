using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models.RuleEngine
{
    public class MessageData
    {
        public string Message { get; set; }
        public Action actions { get; set; }
        public int requestId { get; set; }
        public ApprovalHierarchy approvalHierarchies { get; set; }
        public string status { get; set; }
    }
}
