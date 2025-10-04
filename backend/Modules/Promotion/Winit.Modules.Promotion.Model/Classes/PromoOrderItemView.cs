using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class PromoOrderItemView : PromoOrderItem
    {
        public ActionType ActionType { get; set; }

        // New fields for MPROD configuration support
        public int ConfigGroupId { get; set; }
        public string ConfigName { get; set; }
        public string ConfigPromotionType { get; set; }
    }
}
