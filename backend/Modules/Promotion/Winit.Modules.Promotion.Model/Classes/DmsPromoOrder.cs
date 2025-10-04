using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class DmsPromoOrder
    {
        public DmsPromoOrder()
        {
            this.objPromoCondition = new DmsPromoCondition();
            this.PromoOrderItems = new List<DmsPromoOrderItem>();
            this.PromoOffers = new List<DmsPromoOffer>();
            //this.Attribute = new Dictionary<string, object>();
        }
        //public DmsPromotion Promotion { get; set; }
        public int Id { get; set; }
        public string UID { get; set; }
        public string PromotionUID { get; set; }
        public string SelectionModel { get; set; }
        public string QualificationLevel { get; set; }
        public Nullable<int> MinDealCount { get; set; }
        public Nullable<int> MaxDealCount { get; set; }
        public string ConditionType { get; set; }
        public DmsPromoCondition objPromoCondition { get; set; }
        public List<DmsPromoOrderItem> PromoOrderItems { get; set; }
        public List<DmsPromoOffer> PromoOffers { get; set; }
        //public Dictionary<string, object> Attribute { get; set; }

    }
}
