using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class PromoOrderForSlabs : BaseModel, Interfaces.IPromoOrderForSlabs
    {
        public string? PromoOrderUID { get; set; }
        public bool IsNewOfferType { get; set; }
        public ActionType ActionType { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public decimal Discount { get; set; }
        public bool IsDiscountVisible { get; set; }
        public string DiscountLabel { get; set; }
        public string? OfferTypeUID { get; set; }
        public bool IsOfferTypeDisplay { get; set; }
        public string? OfferTypeLabel { get; set; }
        public bool IsFreeSKUVisible { get; set; }
        public string? FreeSkuUID { get; set; }
        public string? FreeSKULabel { get; set; }
    }
}
