using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Product.Model.Interfaces
{
    public interface IProductType:IBaseModel
    {
        public string ProductTypeName { get; set; }
        public string ProductTypeCode { get; set; }
        public string ProductTypeDescription { get; set; }
        public string ParentProductTypeUID { get; set; }
    }
}
