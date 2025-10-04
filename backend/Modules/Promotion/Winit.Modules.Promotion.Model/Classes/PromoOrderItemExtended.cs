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
    /// Extended PromoOrderItem class with MPROD configuration support
    /// </summary>
    public class PromoOrderItemExtended : PromoOrderItem
    {
        /// <summary>
        /// For MPROD: Configuration group identifier (1, 2, 3, etc.)
        /// </summary>
        public int ConfigGroupId { get; set; }

        /// <summary>
        /// For MPROD: Name of the configuration group
        /// </summary>
        public string ConfigName { get; set; }

        /// <summary>
        /// For MPROD: Promotion type for this config (IQFD, IQPD, IQXF, BQXF)
        /// </summary>
        public string ConfigPromotionType { get; set; }
    }
}