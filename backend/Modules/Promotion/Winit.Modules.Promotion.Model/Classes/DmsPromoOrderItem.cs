using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class DmsPromoOrderItem
    {
        public DmsPromoOrderItem()
        {
            this.objPromoCondition = new DmsPromoCondition();
            //this.lstPromoOrderItem = new List<DmsPromoOrderItem>();
            //this.Attribute = new Dictionary<string, object>();
        }
        //public DmsPromoOrder PromoOrder { get; set; }
        public int Id { get; set; }
        public string UID { get; set; }
        public string PromoOrderUID { get; set; }
        public string ItemCriteriaType { get; set; }
        public string ItemCriteriaSelected { get; set; }
        public string ItemCriteriaSelectedName { get; set; }
        public string ParentUID { get; set; }
        public Nullable<decimal> PromoSplit { get; set; }
        public Nullable<bool> IsCompulsory { get; set; }
        public string ItemUOM { get; set; }
        public string ConditionType { get; set; }
        public DmsPromoCondition objPromoCondition { get; set; }
        //public List<DmsPromoOrderItem> lstPromoOrderItem { get; set; }//Why using
        //public Dictionary<string, object> Attribute { get; set; }

    }
}
