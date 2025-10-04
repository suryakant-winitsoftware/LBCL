using System;
using System.Collections.Generic;
using System.Text;
using Winit.Modules.Base.Model;
using Winit.Modules.Product.Model.Interfaces;

namespace Winit.Modules.Product.Model.Classes
{
    public class ProductTypeBridge : BaseModel, IProductTypeBridge
    {
        public String ProductCode { get; set; }
        public string ProductTypeUID { get; set; }
    }

}
