using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.Model.Interfaces;

namespace Winit.Modules.SKU.Model.Classes
{
    public class TaxSkuMap : BaseModel, ITaxSkuMap
    {
        public string CompanyUID { get; set; }
        public string SKUUID { get; set; }
        public string TaxUID { get; set; }
    }
}
