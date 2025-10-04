using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class ItemPromotionMap : BaseModel, IItemPromotionMap
    {
        public string SKUType { get; set; }
        public string SKUTypeUID { get; set; }
        public string PromotionUID { get; set; }

    }
}
