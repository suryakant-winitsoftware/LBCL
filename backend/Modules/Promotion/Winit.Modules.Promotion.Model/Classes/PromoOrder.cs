using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class PromoOrder : BaseModel, IPromoOrder
    {
        public string PromotionUID { get; set; }
        public string SelectionModel { get; set; }
        public string QualificationLevel { get; set; }
        public int MinDealCount { get; set; }
        public int MaxDealCount { get; set; }

    }
}
