using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

using Winit.Modules.WHStock.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.WHStock.Model.Classes
{
    public class WHStockConversionLine : BaseModel, IWHStockConversionLine
    {
        public string WHStockConversionUID { get; set; }
        public string SKUCode { get; set; }
        public string FromType { get; set; }
        public string ToType { get; set; }
        public decimal? Qty { get; set; }
        public string UOM { get; set; }
        public string ToReason { get; set; }
        public string Remarks { get; set; }
    }
}
