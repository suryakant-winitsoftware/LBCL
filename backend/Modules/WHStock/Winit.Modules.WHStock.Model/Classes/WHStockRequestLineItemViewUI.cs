using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.WHStock.Model.Interfaces;

namespace Winit.Modules.WHStock.Model.Classes
{
    public class WHStockRequestLineItemViewUI : IWHStockRequestLineItemViewUI
    {
        public string SKUUID { get; set; }
        public string SKUCode { get; set; }
        public string SKUName { get; set; }
        public string UID { get; set; }
        public string UOM1 { get; set; }
        public string UOM2 { get; set; }
        public string UOM { get; set; }
        public decimal RequestedQty1 { get; set; }
        public decimal RequestedQty2 { get; set; }
        public decimal RequestedQty { get; set; }
        public decimal CPEApprovedQty1 { get; set; }
        public decimal CPEApprovedQty2 { get; set; }
        public decimal CPEApprovedQty { get; set; }
        public decimal ApprovedQty1 { get; set; }
        public decimal ApprovedQty2 { get; set; }
        public decimal ApprovedQty { get; set; }
        public decimal CollectedQty1 { get; set; }
        public decimal CollectedQty2 { get; set; }
        public decimal CollectedQty { get; set; }
        public decimal ForwardQty1 { get; set; }
        public decimal ForwardQty2 { get; set; }
        public decimal ForwardQty { get; set; }
        public decimal TemplateQty1 { get; set; }
        public decimal TemplateQty2 { get; set; }
        public decimal UOM2CNF { get; set; }
        public decimal UOM1CNF { get; set; }
        public bool IsSelected { get; set; }
        public int LineNumber { get; set; }
        public string OrgUID { get; set; }
        public string WareHouseUID { get; set; }
        public int YearMonth { get; set; }
    }
}
