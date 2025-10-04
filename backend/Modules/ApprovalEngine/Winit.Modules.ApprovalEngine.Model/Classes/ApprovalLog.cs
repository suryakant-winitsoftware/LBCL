using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ApprovalEngine.Model.Interfaces;

namespace Winit.Modules.ApprovalEngine.Model.Classes
{
    public class ApprovalLog : IApprovalLog
    {
        public string? ApproverId { get; set; }
        public int? Level { get; set; }
        public string? Status { get; set; }
        public string? Comments { get; set; }
        public string? ModifiedBy { get; set; }
        public string? ReassignTo { get; set; }
        public string? CreatedOn { get; set; }
    }
}
