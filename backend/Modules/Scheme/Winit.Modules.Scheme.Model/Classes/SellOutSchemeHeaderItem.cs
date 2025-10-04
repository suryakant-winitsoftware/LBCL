using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Interfaces;

namespace Winit.Modules.Scheme.Model.Classes
{
    public class SellOutSchemeHeaderItem : SellOutSchemeLine,ISellOutSchemeHeaderItem
    {
        // UI Properties
        public string ChannelPartner { get; set; }
        public decimal AvailableKittyProvisionAmount { get; set; }
        public decimal ProposedSellOutCreditNoteAmount { get; set; }
        public decimal BranchContribution { get; set; }
        public decimal HOContribution { get; set; }
        public List<ISellOutSchemeLine> SchemeLines { get; set; }

        // Properties for data retrieval
        public string OrgUid { get; set; }
        public string FranchiseeOrgUid { get; set; }
        public decimal ContributionLevel1 { get; set; }
        public decimal ContributionLevel2 { get; set; }
        public decimal TotalCreditNote { get; set; }
        public decimal AvailableProvision2Amount { get; set; }
        public decimal AvailableProvision3Amount { get; set; }
        public decimal StandingProvisionAmount { get; set; }
        public string JobPositionUid { get; set; }
        public string EmpUid { get; set; }
        public int LineCount { get; set; }
        public string Status { get; set; }


    }
}
