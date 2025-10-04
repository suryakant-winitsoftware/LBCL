using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.SKU.Model.Interfaces
{
    public interface ITaxSkuMap : IBaseModel
    {
        public string CompanyUID { get; set; }
        public string SKUUID { get; set; }
        public string TaxUID { get; set; }
    }
}
