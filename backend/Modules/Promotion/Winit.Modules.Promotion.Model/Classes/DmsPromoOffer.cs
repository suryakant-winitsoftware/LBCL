using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class DmsPromoOffer
    {
        public DmsPromoOffer()
        {
            this.objPromoCondition = new DmsPromoCondition();
            this.PromoOfferItems = new List<DmsPromoOfferItem>();
            //this.Attribute = new Dictionary<string, object>();
        }

        public int Id { get; set; }
        public string UID { get; set; }
        public string PromoOrderUID { get; set; }
        public string Type { get; set; }
        public string QualificationLevel { get; set; }
        public string ApplicationLevel { get; set; }
        public string SelectionModel { get; set; }
        public string ConditionType { get; set; }
        public Nullable<bool> HasOfferItemSelection { get; set; }
        public DmsPromoCondition objPromoCondition { get; set; }
        public List<DmsPromoOfferItem> PromoOfferItems { get; set; }
        //public Dictionary<string, object> Attribute { get; set; }

    }
}
