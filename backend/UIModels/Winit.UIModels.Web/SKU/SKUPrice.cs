using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Enums;

namespace Winit.UIModels.Web.SKU
{
    public class SKUPrice:BaseModel
    {
        public string SKUPriceListUID { get; set; }
        public string SKUCode { get; set; }
        public string UOM { get; set; }
        public decimal Price { get; set; }
        public decimal DefaultWSPrice { get; set; }
        public decimal DefaultRetPrice { get; set; }
        public decimal DummyPrice { get; set; }
        public decimal MRP { get; set; }
        public decimal PriceUpperLimit { get; set; }
        public decimal PriceLowerLimit { get; set; }
        public string Status { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime DummyValidFrom { get; set; }
        public DateTime ValidUpto { get; set; }
        public DateTime DummyValidUpto { get; set; }
        public bool IsActive { get; set; }
        public bool IsTaxIncluded { get; set; }
        public string VersionNo { get; set; }
        public string SKUUID { get; set; }
        public ActionType ActionType { get; set; }


        public bool IsShowDummyRow { get; set; }
        public bool IsEdit { get; set; }
        public bool IsSaved { get; set; }
        public bool ISExpired { get; set; }
        public bool ISDuplicate { get; set; }
        public bool ISNew { get; set; }
        public bool IsModified { get; set; }
        public int IsLatest { get; set; }

    
    }
}
