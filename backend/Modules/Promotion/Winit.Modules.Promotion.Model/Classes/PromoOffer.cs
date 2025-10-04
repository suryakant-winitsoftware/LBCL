using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class PromoOffer : BaseModel, IPromoOffer
    {
        public string PromoOrderUID { get; set; }
        public string PromotionUID { get; set; }
        public string Type { get; set; }
        public string QualificationLevel { get; set; }
        public string ApplicationLevel { get; set; }
        public string SelectionModel { get; set; }
        public bool HasOfferItemSelection { get; set; }

    }
}
