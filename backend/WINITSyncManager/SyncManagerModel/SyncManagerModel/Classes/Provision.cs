using SyncManagerModel.Base;
using SyncManagerModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerModel.Classes
{
    public class Provision:SyncBaseModel,IProvision
    {
        public long SyncLogId { get; set; }
        public string? UID { get; set; }
        public string? ProvisionId { get; set; }
        public string? CustomerCode { get; set; }
        public string? Branch { get; set; }
        public string? SalesOffice { get; set; }
        public string? OracleOrderNumber { get; set; }
        public string? DeliveryId { get; set; }
        public string? GstInvoiceNumber { get; set; }
        public string? ArNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string? ItemCode { get; set; }
        public int Qty { get; set; }
        public string? SchemeType { get; set; }
        public decimal SchemeAmount { get; set; }
        public string? Naration { get; set; }
    }
}
