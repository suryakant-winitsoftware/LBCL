using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Product.Model.Interfaces;

namespace Winit.Modules.Product.Model.Classes
{
    public class ProductUOM:BaseModel,IProductUOM
    {
        public Int64 ProductUOMId { get; set; }
        public string ProductCode { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string BarCode { get; set; }
        public bool IsBaseUOM { get; set; }
        public bool IsOuterUOM { get; set; }
        public decimal Multiplier { get; set; }
    }
}
