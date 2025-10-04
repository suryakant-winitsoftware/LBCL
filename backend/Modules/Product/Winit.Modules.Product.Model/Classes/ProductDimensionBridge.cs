using System;
using System.Collections.Generic;
using System.Text;
using Winit.Modules.Base.Model;
using Winit.Modules.Product.Model.Interfaces;

namespace Winit.Modules.Product.Model.Classes
{
    public class ProductDimensionBridge : BaseModel, IProductDimensionBridge
    {
        public string ProductCode { get; set; }
        public string ProductDimensionUID { get; set; }
    }

}
