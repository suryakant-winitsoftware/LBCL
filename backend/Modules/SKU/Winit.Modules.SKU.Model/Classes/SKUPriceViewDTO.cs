using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.Model.Interfaces;

namespace Winit.Modules.SKU.Model.Classes
{
    public class SKUPriceViewDTO 
    {
        public SKUPriceList SKUPriceGroup { get; set; }
        public List<SKUPrice> SKUPriceList { get; set; }


    }
}
