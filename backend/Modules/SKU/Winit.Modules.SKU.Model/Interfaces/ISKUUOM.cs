using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.SKU.Model.Interfaces
{
    public interface ISKUUOM : IBaseModel
    {
        public string SKUUID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Barcodes { get; set; }
        public bool IsBaseUOM { get; set; }
        public bool IsOuterUOM { get; set; }
        public decimal Multiplier { get; set; }
        public decimal Length { get; set; }
        public decimal Depth { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public decimal Volume { get; set; }
        public decimal Weight { get; set; }
        public decimal GrossWeight { get; set; }
        public string DimensionUnit { get; set; }
        public string VolumeUnit { get; set; }
        public string WeightUnit { get; set; }
        public string GrossWeightUnit { get; set; }
        public decimal Liter { get; set; }
        public decimal KGM { get; set; }
    }
}
