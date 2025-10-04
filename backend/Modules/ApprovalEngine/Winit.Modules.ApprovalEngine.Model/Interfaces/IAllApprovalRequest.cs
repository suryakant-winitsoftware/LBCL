using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Winit.Modules.ApprovalEngine.Model.Interfaces
{
    public interface IAllApprovalRequest
    {
         long Id { get; set; }
         string? LinkedItemType { get; set; }
         string? LinkedItemUID { get; set; }
         string? RequestID { get; set; }

        public string? ApproverID { get; set; }
        public string? Level { get; set; }
        public string? NextApproverID { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
        public string? Name { get; set; }
        public string? ApprovalUserDetail { get; set; }
    }
}
