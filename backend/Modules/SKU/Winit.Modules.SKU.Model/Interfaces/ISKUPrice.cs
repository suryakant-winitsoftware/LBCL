using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.Model.Interfaces
{
    public interface ISKUPrice:IBaseModel
    {
        public string SKUName { get; set; }
        public string SKUPriceListUID { get; set; }
        public string SKUCode { get; set; }
        public string UOM { get; set; }
        public decimal Price { get; set; }
        public decimal TempPrice { get; set; }
        public decimal DefaultWSPrice { get; set; }
        public decimal TempDefaultWSPrice { get; set; }
        public decimal DefaultRetPrice { get; set; }
        public decimal TempDefaultRetPrice { get; set; }
        public decimal DummyPrice { get; set; }
        public decimal TempDummyPrice { get; set; }
        public decimal MRP { get; set; }
        public decimal TempMRP { get; set; }
        public decimal PriceUpperLimit { get; set; }
        public decimal PriceLowerLimit { get; set; }
        public decimal TempPriceLowerLimit { get; set; }
        public string Status { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUpto { get; set; }
        public bool IsActive { get; set; }
        public bool IsTaxIncluded { get; set; }
        public string VersionNo { get; set; }
        public string SKUUID { get; set; }
        public ActionType ActionType { get; set; }
        public int IsLatest { get; set; }
        public decimal LadderingAmount { get; set; }
        public decimal LadderingPercentage { get; set; }
        public DateTime TempValidFrom { get; set; }
        public DateTime TempValidUpto { get; set; }
        public bool IsEdit { get; set; }
        public bool IsModified { get; set; }
        public bool ISDuplicate { get; set; }
        public bool ISNew { get; set; }
    }
}
