using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class PromoCondition : BaseModel, IPromoCondition
    {
        public string PromotionUID { get; set; }
        public string ReferenceType { get; set; }
        public string ReferenceUID { get; set; }
        public string ConditionType { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public int MaxDealCount { get; set; }
        public string UOM { get; set; }
        public bool AllUOMConversion { get; set; }
        public string ValueType { get; set; }
        public bool IsProrated { get; set; }
        public string DiscountType { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? DiscountAmount { get; set; }
        public int? FreeQuantity { get; set; }

    }
}
