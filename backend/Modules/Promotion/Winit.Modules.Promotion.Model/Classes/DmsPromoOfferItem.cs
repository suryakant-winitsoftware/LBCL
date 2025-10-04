using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class DmsPromoOfferItem
    {
        public DmsPromoOfferItem()
        {
            //this.Attribute = new Dictionary<string, object>();
            this.objPromoCondition = new DmsPromoCondition();
        }
        public int Id { get; set; }
        public string UID { get; set; }
        public string PromoOfferUID { get; set; }
        public string ItemCriteriaType { get; set; }
        public string ItemCriteriaSelected { get; set; }
        public Nullable<bool> IsCompulsory { get; set; }
        public string ItemUOM { get; set; }
        public string ConditionType { get; set; }
        public DmsPromoCondition objPromoCondition { get; set; }
        //public Dictionary<string, object> Attribute { get; set; }
    }
}
