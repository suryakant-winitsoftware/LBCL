using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Scheme.Model.Interfaces;

namespace Winit.Modules.Scheme.Model.Classes
{
    public class SellInSchemeHeader : BaseModel, ISellInSchemeHeader
    {
        public string OrgUID { get; set; }
        public string Code { get; set; }
        public string FranchiseeOrgUID { get; set; }
        public int MaxDealCount { get; set; }
        public string RequestType { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? ValidUpTo { get; set; }
        public decimal AvailableProvision2Amount { get; set; }
        public decimal AvailableProvision3Amount { get; set; }
        public decimal StandingProvisionAmount { get; set; }
        public string JobPositionUID { get; set; }
        public string EmpUID { get; set; }
        public int LineCount { get; set; }
        public string Status { get; set; }
        public bool IsTillMonthEnd { get; set; }
        public bool IsApprovalCreated { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedTime { get; set; }
    }
}
