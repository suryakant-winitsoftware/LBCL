using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WINITMobile.Pages.PriceCheck.Models
{
    public class CapturePOItem
    {
        public string SKUCode { get; set; }
        public string Quantity { get; set; }
        public string PONumber { get; set; }
        public string SKUName { get; set; }
        public string SKUImage { get; set; }
        public string UOM { get; set; }
        public decimal LastVisitPrice { get; set; }
        public decimal RSP { get; set; }
        public decimal ShelfPrice { get; set; }
        public string StoreUID { get; set; }
        public string SKUUID { get; set; }
        public int Qty { get; set; }
        public decimal Value { get; set; }
        public bool IsSelected { get; set; }
        public decimal TotalPOValue => Qty * Value;
    }
}
