using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.WHStock.Model.Interfaces
{
    public interface IWHStockConversionLine : IBaseModel
    {
         string WHStockConversionUID { get; set; }
         string SKUCode { get; set; }
         string FromType { get; set; }
         string ToType { get; set; }
         decimal? Qty { get; set; }
         string UOM { get; set; }
         string ToReason { get; set; }
         string Remarks { get; set; }

    }
}
