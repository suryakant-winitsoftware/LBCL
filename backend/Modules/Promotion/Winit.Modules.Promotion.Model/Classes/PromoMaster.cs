using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class PromoMaster :  IPromoMaster
    {
        public IPromotion Promotion { get; set; }
        public List<IPromoOrder> PromoOrderList { get; set; }
        public List<IPromoOrderItem> PromoOrderItemList { get; set; }
        public List<IPromoOffer> PromoOfferList { get; set; }
        public List<IPromoOfferItem> PromoOfferItemList { get; set; }
        public List<IPromoCondition> PromoConditionList { get; set; }
        public List<IItemPromotionMap> ItemPromotionMapList { get; set; }

    }
}
