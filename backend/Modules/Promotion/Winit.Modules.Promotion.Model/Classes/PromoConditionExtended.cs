using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;

namespace Winit.Modules.Promotion.Model.Classes
{
    /// <summary>
    /// Extended PromoCondition class with MPROD configuration support
    /// </summary>
    public class PromoConditionExtended : PromoCondition
    {
        /// <summary>
        /// For MPROD: Links to config_group_id in promo_order_item
        /// </summary>
        public int ConfigGroupId { get; set; }

        /// <summary>
        /// For MPROD: JSON string with configuration details
        /// </summary>
        public string ConfigDetails { get; set; }
    }
}