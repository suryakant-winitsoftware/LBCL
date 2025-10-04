using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Provisioning.Model.Interfaces
{
    public interface IProvisionApproval : IBaseModel
    {
        //Summary View
        public string? ChannelPartner { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerCode { get; set; }
        public string? ProvisionType { get; set; }
        public string? StoreUID { get; set; }
        public decimal? Amount { get; set; }

        //Detail View
        public string? InvoiceNumber { get; set; }
        public string? ArNo { get; set; }
        public string? ItemCode { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public int? Quantity { get; set; }
        public string? Naration { get; set; }
        public string? Remarks { get; set; }
        public string? Status { get; set; }

        //Only for Confirmed Tab
        public string? CnNumber { get; set; }
        public DateTime? CnDate { get; set; }
        public decimal? CnAmount { get; set; }
        public string? ProvisionIDs { get; set; }
        public string? RequestedByEmpUID { get; set; }
        public DateTime? RequestedDate { get; set; }
    }
}
