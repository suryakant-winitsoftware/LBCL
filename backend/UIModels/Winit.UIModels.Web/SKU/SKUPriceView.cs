using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Web.SKU
{
    public class SKUPriceView
    {
        public Winit.Modules.SKU.Model.Interfaces.ISKUPriceList SKUPriceGroup { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> SKUPriceList { get; set; }
    }
}
