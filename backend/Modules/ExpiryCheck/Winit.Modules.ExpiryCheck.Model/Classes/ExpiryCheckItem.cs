using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.ExpiryCheck.Model.Classes
{
    public class ExpiryCheckItem
    {
        public string SKUCode { get; set; }
        public string Quantity { get; set; }
        public string SKUName { get; set; }
        public string SKUImage { get; set; }
        public string UOM { get; set; }
        public decimal LastVisitPrice { get; set; }
        public decimal RSP { get; set; }
        public decimal ShelfPrice { get; set; }
        public string StoreUID { get; set; }
        public string SKUUID { get; set; }
        public decimal Qty { get; set; }
        public bool IsSelected { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public ExpiryCheckItem DeepClone()
        {
            return new ExpiryCheckItem
            {
                SKUCode = this.SKUCode,
                SKUName = this.SKUName,
                SKUImage = this.SKUImage,
                Qty = this.Qty,
                ExpiryDate = this.ExpiryDate
            };
        }
    }
}
