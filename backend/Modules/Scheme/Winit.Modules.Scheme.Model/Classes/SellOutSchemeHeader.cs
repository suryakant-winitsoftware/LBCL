using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Scheme.Model.Interfaces;

namespace Winit.Modules.Scheme.Model.Classes
{
    public class SellOutSchemeHeader:BaseModel, ISellOutSchemeHeader
    {
        public string OrgUID { get; set; }
        public string Remarks { get; set; }
        public string Code { get; set; }
        public string FranchiseeOrgUID { get; set; }
        public decimal? ContributionLevel1 { get; set; }
        public decimal? ContributionLevel2 { get; set; }
        public decimal? ContributionLevel3 { get; set; }
        public decimal? TotalCreditNote { get; set; }
        public decimal? AvailableProvision2Amount { get; set; }
        public decimal? AvailableProvision3Amount { get; set; }
        public decimal? StandingProvisionAmount { get; set; }
        public string JobPositionUID { get; set; }
        public string EmpUID { get; set; }
        public int LineCount { get; set; }
        public string Status { get; set; }
        public bool IsApprovalCreated { get; set; }
    }
}
