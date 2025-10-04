using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Store.Model.Interfaces
{
    public  interface IStorePromotionMap
    {
        public string id { get; set; }
        public string ss { get; set; }
        public DateTime updated_on { get; set; }
        public string store_uid { get; set; }
        public string promotion_uids { get; set; }
    }
}
