using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Common.SKU
{
    public class SKUUOMView
    {
        public required string SKUCode { get; set; }
        public required string Code { get; set; }
        public required string Name { get; set; }
        public required string Label { get; set; }
        public string? Barcode { get; set; }
        public bool IsBaseUOM { get; set; }
        public bool IsOuterUOM { get; set; }
        public decimal Multiplier { get; set; }
    }
}
