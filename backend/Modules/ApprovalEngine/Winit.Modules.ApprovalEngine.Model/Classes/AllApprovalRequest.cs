using System;
using System.Collections.Generic;
using System.Text;
using Winit.Modules.Base.Model;

namespace Winit.Modules.ApprovalEngine.Model.Classes
{
    public class AllApprovalRequest : Interfaces.IAllApprovalRequest
    {
        public long Id { get; set; }
        public string? LinkedItemType { get; set; }
        public string? LinkedItemUID { get; set; }
        public string? RequestID { get; set; }
        public string? ApproverID { get; set; }
        public string? Level { get; set; }
        public string? NextApproverID { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
        public string? Name { get; set; }
        public string? ApprovalUserDetail { get; set; }

    }
   
}
