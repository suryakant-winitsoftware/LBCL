using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Product.Model.Interfaces
{
    public interface IProductDimension : IBaseModel
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string ParentUID { get; set; }
    }
}
