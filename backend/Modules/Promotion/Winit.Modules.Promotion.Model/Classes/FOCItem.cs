using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class FOCItem
    {
        public string PromotionUID { get; set; }
        public string ItemCode { get; set; }
        public string UOM { get; set; }
        public decimal Qty { get; set; }
    }
}
