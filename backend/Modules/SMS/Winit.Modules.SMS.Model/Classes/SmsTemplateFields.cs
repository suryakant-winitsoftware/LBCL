using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SMS.Model.Interfaces;

namespace Winit.Modules.SMS.Model.Classes
{
    public class SmsTemplateFields : ISmsTemplateFields
    {
        public string ChannelPartnerName { get; set; }
        public string OrderNo { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceQty { get; set; }
        public string InvoiceValue { get; set; }
        public string OrderQty { get; set; }
        public string RequestedQty { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedQty { get; set; }
        public string OrderValue { get; set; }
        public string CreatedBy { get; set; }
        public string ChannelPartnerMobileNo { get; set; }
        public string ChannelPartnerEmail{ get; set; }
        public string ASMMobileNo { get; set; }
        public string ASMName { get; set; }
        public string ASMEmail { get; set; }
        public string BMMobileNo { get; set; }
        public string BMEmail { get; set; }
        public string Url { get; set; }
    }
}
