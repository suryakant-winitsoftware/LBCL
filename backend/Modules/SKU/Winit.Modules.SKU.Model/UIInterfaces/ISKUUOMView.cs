using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.SKU.Model.UIInterfaces
{
    public interface ISKUUOMView
    {
        public string SKUUID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string? Barcode { get; set; }
        public bool IsBaseUOM { get; set; }
        public bool IsOuterUOM { get; set; }
        public decimal Multiplier { get; set; }
    }
}
