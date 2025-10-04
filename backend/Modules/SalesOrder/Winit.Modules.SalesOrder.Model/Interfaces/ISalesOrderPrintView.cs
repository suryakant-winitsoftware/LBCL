using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Constants;

namespace Winit.Modules.SalesOrder.Model.Interfaces
{
    public interface ISalesOrderPrintView
    {
        public string SalesOrderNumber { get; set; }
        public string Status { get; set; }
        public string OrderType { get; set; }
        public string StoreCode { get; set; }
        public string StoreNumber { get; set; }
        public string StoreName { get; set; }
        public string CustomerPO { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ExpectedDeliveryDate { get; set; }
        public DateTime DeliveredDateTime { get; set; }
        public string CurrencySymbol { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal NetAmount { get; set; }
        public decimal LineCount { get; set; }
        public decimal QtyCount { get; set; }
        public decimal TotalLineDiscount { get; set; }
        public decimal TotalCashDiscount { get; set; }
        public decimal TotalHeaderDiscount { get; set; }
        public decimal TotalExciseDuty { get; set; }
        public decimal TotalLineTax { get; set; }
        public decimal TotalHeaderTax { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
    }
}
