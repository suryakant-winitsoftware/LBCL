using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class DmsPromoCondition
    {
        public int Id { get; set; }
        public string UID { get; set; }
        public string ReferenceType { get; set; }
        public string ReferenceUID { get; set; }
        public string ConditionType { get; set; }
        public Nullable<decimal> Min { get; set; }
        public Nullable<decimal> Max { get; set; }
        public Nullable<int> MaxDealCount { get; set; }
        public string UOM { get; set; }
        public Nullable<bool> AllUOMConversion { get; set; }
        public string ValueType { get; set; }
        public Nullable<bool> IsProrated { get; set; }
        public Nullable<int> ss { get; set; }
        public Nullable<System.DateTime> CreatedTime { get; set; }
        public Nullable<System.DateTime> ModifiedTime { get; set; }
        public Nullable<System.DateTime> ServerAddTime { get; set; }
        public Nullable<System.DateTime> ServerModifiedTime { get; set; }

    }
}
