using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Scheme.Model.Interfaces
{
    public interface ISellInSchemeHeader : IBaseModel
    {
        string OrgUID { get; set; }
        string Code { get; set; }
        string FranchiseeOrgUID { get; set; }
        int MaxDealCount { get; set; }
        string RequestType { get; set; }
        DateTime? ValidFrom { get; set; }
        DateTime? EndDate { get; set; }
        DateTime? ValidUpTo { get; set; }
        decimal AvailableProvision2Amount { get; set; }
        decimal AvailableProvision3Amount { get; set; }
        decimal StandingProvisionAmount { get; set; }
        string JobPositionUID { get; set; }
        string EmpUID { get; set; }
        int LineCount { get; set; }
        string Status { get; set; }
        bool IsTillMonthEnd { get; set; }
        bool IsApprovalCreated { get; set; }
        string ApprovedBy { get; set; }
        DateTime ApprovedTime { get; set; }
    }
}
