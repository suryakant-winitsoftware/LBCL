using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Product.Model.Interfaces
{
    public interface IProductDimensionBridge : Base.Model.IBaseModel
    {
        public string ProductCode { get; set; }
        public string ProductDimensionUID { get; set; }
    }
}
