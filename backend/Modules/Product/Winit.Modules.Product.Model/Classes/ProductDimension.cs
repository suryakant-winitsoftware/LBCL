using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Product.Model.Interfaces;

namespace Winit.Modules.Product.Model.Classes
{
    public class ProductDimension:BaseModel,IProductDimension
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string ParentUID { get; set; }
    }
}
