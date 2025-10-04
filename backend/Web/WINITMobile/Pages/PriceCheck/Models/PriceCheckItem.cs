using System;

namespace WINITMobile.Pages.PriceCheck.Models
{
    public class PriceCheckItem
    {
        public string SKUCode { get; set; }
        public string SKUName { get; set; }
        public string SKUImage { get; set; }
        public string UOM { get; set; }
        public decimal LastVisitPrice { get; set; }
        public decimal RSP { get; set; }
        public decimal ShelfPrice { get; set; }
        public string StoreUID { get; set; }
        public string SKUUID { get; set; }
        public string Qty { get; set; }
        public bool IsSelected { get; set; }
        public string ExpiryDate { get; set; }
    }
} 