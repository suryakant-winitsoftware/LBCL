using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Invoice.Model.Interfaces;

namespace Winit.Modules.Invoice.Model.Classes
{
    public class ProvisioningCreditNoteView : IProvisioningCreditNoteView
    {
        public string UID { get; set; }
        public string ChannelPartnerCode { get; set; }
        public string ChannelPartnerName { get; set; }
        public string BranchUID { get; set; }
        public string HOOrgUID { get; set; }
        public string InvoiceUID { get; set; }
        public string OrgUID { get; set; }
        public string InvoiceNumber { get; set; }
        public string PONumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal CreditNoteAmount1 { get; set; }
        public decimal CreditNoteAmount2 { get; set; }
        public decimal CreditNoteAmount3 { get; set; }
        public decimal CreditNoteAmount4 { get; set; }
        public bool Status { get; set; }
        public string ApprovedByEmpUID { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public bool IsSelected { get; set; }

        public string LinkedItemType { get; set; }
        public string LinkedItemUid { get; set; }
    }
}
