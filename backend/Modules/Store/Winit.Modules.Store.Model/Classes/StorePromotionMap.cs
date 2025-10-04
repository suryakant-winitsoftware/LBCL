using System;
using System.Collections.Generic;
using System.Text;
using Winit.Modules.Base.Model;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
{
    public class StorePromotionMap : BaseModel, IStorePromotionMap
    {
        public string id { get; set; }
        public string ss { get; set; }
        public DateTime updated_on { get; set; }
        public string store_uid { get; set; }
        public string promotion_uids { get; set; }
    }
}
