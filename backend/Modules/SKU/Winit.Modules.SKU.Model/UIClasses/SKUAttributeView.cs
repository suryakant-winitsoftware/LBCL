using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.UIInterfaces;

namespace Winit.Modules.SKU.Model.UIClasses
{
    public class SKUAttributeView: ISKUAttributeView
    {
        public required string SKUUID { get; set; }
        public required string Name { get; set; }
        public required string Code { get; set; }
        public required string Value { get; set; }
    }
}
