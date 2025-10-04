using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class DmsPromotion 
    {
        public DmsPromotion()
        {
            this.PromoOrders = new List<DmsPromoOrder>();
            //this.Attribute = new Dictionary<string, object>();
        }
        public int Id { get; set; }
        public string UID { get; set; }
        public string OrgUID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Remarks { get; set; }
        public string Category { get; set; }
        public Nullable<bool> HasSlabs { get; set; }
        public string CreatedByEmpUID { get; set; }
        public Nullable<System.DateTime> ValidFrom { get; set; }
        public Nullable<System.DateTime> ValidUpto { get; set; }
        public string Type { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string PromoFormat { get; set; }
        public string PromoFormatName { get; set; }
        public int Priority { get; set; }
        public bool IsItemLevelApplicable = false;
        public List<DmsPromoOrder> PromoOrders { get; set; }        
        //public Dictionary<string, object> Attribute { get; set; }
    }
}
