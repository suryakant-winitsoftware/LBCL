using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Product.Model.Interfaces;

namespace Winit.Modules.Product.Model.Classes
{
    public class ProductType:BaseModel, IProductType
    {
        public string ProductTypeName { get; set; }
        public string ProductTypeCode { get; set; }
        public string ProductTypeDescription { get; set; }
        public string ParentProductTypeUID { get; set; }
    }
}
