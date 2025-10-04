using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Product.Model.Interfaces
{
    public interface IProductAttributes:IBaseModel
    {
        public string ProductCode { get; set; }
        public string HierachyType { get; set; }
        public string HierachyCode { get; set; }
        public string HierachyValue { get; set; }
    }
}
