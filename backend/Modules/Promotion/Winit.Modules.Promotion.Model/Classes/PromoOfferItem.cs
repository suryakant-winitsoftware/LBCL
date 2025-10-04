using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class PromoOfferItem : BaseModel, IPromoOfferItem
    {
        public string PromoOfferUID { get; set; }
        public string PromotionUID { get; set; }
        public string ItemCriteriaType { get; set; }
        public string ItemCriteriaSelected { get; set; }
        public bool IsCompulsory { get; set; }
        public string ItemUOM { get; set; }
        public decimal? Quantity { get; set; }

    }
}
