using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.SKU.Model.Interfaces
{
    public interface ISKUToGroupMapping : IBaseModel
    {
        public string SKUUID { get; set; }
        public string SKUGroupUID { get; set; }
    }
}
