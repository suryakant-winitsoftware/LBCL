
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.ReturnOrder.Model.Interfaces;

namespace Winit.Modules.ReturnOrder.Model.Classes
{
    public class ReturnOrder:BaseModel,IReturnOrder
    {
        public string ReturnOrderNumber { get; set; }
        public string DraftOrderNumber { get; set; }
        public string JobPositionUID { get; set; }
        public string EmpUID { get; set; }
        public string OrgUID { get; set; }
        public string DistributionChannelUID { get; set; }
        public string StoreUID { get; set; }
        public bool IsTaxApplicable { get; set; }
        public string RouteUID { get; set; }
        public string BeatHistoryUID { get; set; }
        public string StoreHistoryUID { get; set; }
        public string Status { get; set; }
        public string OrderType { get; set; }
        public DateTime OrderDate { get; set; }
        public string CurrencyUID { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalLineDiscount { get; set; }
        public decimal TotalCashDiscount { get; set; }
        public decimal TotalHeaderDiscount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalExciseDuty { get; set; }
        public decimal LineTaxAmount { get; set; }
        public decimal HeaderTaxAmount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TotalFakeAmount { get; set; }
        public int LineCount { get; set; }
        public decimal QtyCount { get; set; }
        public string Notes { get; set; }
        public bool IsOffline { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string DeliveredByOrgUID { get; set; }
        public string Source { get; set; }
        public string PromotionUID { get; set; }
        public decimal TotalLineTax { get; set; }
        public decimal TotalHeaderTax { get; set; }
        public string SalesOrderUID { get; set; }
    }
}
