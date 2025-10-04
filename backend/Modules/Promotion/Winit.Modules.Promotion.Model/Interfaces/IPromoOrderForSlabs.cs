using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Promotion.Model.Interfaces
{
    public interface IPromoOrderForSlabs:IBaseModel
    {
        bool IsNewOfferType { get; set; }
        string? PromoOrderUID { get; set; }
        ActionType ActionType { get; set; }
        decimal Min { get; set; }
        decimal Max { get; set; }
        decimal Discount { get; set; }
        bool IsDiscountVisible { get; set; }
        string DiscountLabel { get; set; }
        string? OfferTypeUID { get; set; }
        bool IsOfferTypeDisplay { get; set; }
        string? OfferTypeLabel { get; set; }
        bool IsFreeSKUVisible { get; set; }
        string? FreeSkuUID { get; set; }
        string? FreeSKULabel { get; set; }
    }
}
