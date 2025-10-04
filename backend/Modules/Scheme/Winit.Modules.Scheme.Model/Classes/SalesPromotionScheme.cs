using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Scheme.Model.Interfaces;

namespace Winit.Modules.Scheme.Model.Classes
{
    public class SalesPromotionScheme:BaseModel,ISalesPromotionScheme
    {
        public string UID { get; set; }
        public string OrgUID { get; set; }
        public string Code { get; set; }
        public string FranchiseeOrgUID { get; set; }
        public string ChannelPartnerName { get; set; }
        public decimal? ContributionLevel1 { get; set; }
        public decimal? ContributionLevel2 { get; set; }
        public decimal? ContributionLevel3 { get; set; }
        public string? ExectionNotes { get; set; }
        public decimal? AvailableProvision2Amount { get; set; }
        public decimal? AvailableProvision3Amount { get; set; }
        public decimal? StandingProvisionAmount { get; set; }
        public string JobPositionUID { get; set; }
        public string EmpUID { get; set; }
        public string Status { get; set; }
        public string ActivityType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal? Amount { get; set; }
        public string Description { get; set; }
        public string PoNumber { get; set; }
        public DateTime? PoDate { get; set; }
        public string Remarks { get; set; }
        public bool IsPOHandledByDMS { get; set; }
        public bool IsApprovalCreated { get; set; }
    }
}
