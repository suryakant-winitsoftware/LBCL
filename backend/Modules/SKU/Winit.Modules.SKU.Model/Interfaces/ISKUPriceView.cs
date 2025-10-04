using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.Model.Classes;

namespace Winit.Modules.SKU.Model.Interfaces
{
    public interface ISKUPriceView 
    {
        public ISKUPriceList SKUPriceGroup { get; set; }
        public List<ISKUPrice> SKUPriceList { get; set; }
    }
}
