using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class PromotionItemView
    {
        public string UniqueUId = Guid.NewGuid().ToString();
        public string SKUUID { get; set; }
        public bool IsCartItem { get; set; }
        public string UOM { get; set; }
        public decimal Multiplier { get; set; }
        public decimal Qty { get; set; }
        public decimal QtyBU { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public string ItemType { get; set; }
        public string ChildType = string.Empty;
        public bool IsDiscountApplied = true;
        public decimal BasePrice { get; set; }
        public decimal ReplacePrice { get; set; }
        public decimal UnitPrice { get; set; }
        public List<PromotionItemView> SalesOrderItemListFOC = null;
        public string PromotionUID { get; set; }
        //public Dictionary<string, Sku.SKUAttribute>? Attributes { get; set; }
        public Dictionary<string, SKU.Model.Classes.SKUAttributes>? Attributes { get; set; }
    }
}
