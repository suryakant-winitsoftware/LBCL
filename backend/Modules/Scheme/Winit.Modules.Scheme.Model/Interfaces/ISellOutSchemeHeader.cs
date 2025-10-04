using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Scheme.Model.Interfaces
{
    public interface ISellOutSchemeHeader:IBaseModel
    {
         string OrgUID { get; set; }
         string Remarks { get; set; }
         string Code { get; set; }
         string FranchiseeOrgUID { get; set; }
         decimal? ContributionLevel1 { get; set; } 
         decimal? ContributionLevel2 { get; set; }
         decimal? ContributionLevel3 { get; set; }
         decimal? TotalCreditNote { get; set; }
         decimal? AvailableProvision2Amount { get; set; }
         decimal? AvailableProvision3Amount { get; set; }
         decimal? StandingProvisionAmount { get; set; }
         string JobPositionUID { get; set; }
         string EmpUID { get; set; }
         int LineCount { get; set; }
         string Status { get; set; }
        bool IsApprovalCreated { get; set; }
    }
}
