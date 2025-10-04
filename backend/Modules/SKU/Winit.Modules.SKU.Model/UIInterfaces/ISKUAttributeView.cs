using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.SKU.Model.UIInterfaces
{
    public interface ISKUAttributeView
    {
        public string SKUUID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
    }
}
