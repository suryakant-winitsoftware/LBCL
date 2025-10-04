using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Product.Model.Interfaces
{
    public interface IProductTypeBridge : Base.Model.IBaseModel
    {
        public String ProductCode { get; set; }
        public string ProductTypeUID { get; set; }
    }
}
