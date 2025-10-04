using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.WHStock.Model.Interfaces
{
    public interface IWHStockConversion : IBaseModel
    {
         string CompanyUID { get; set; }
         string WarehouseUID { get; set; }
         string OrgUID { get; set; }
         string Status { get; set; }
         string JobPositionUID { get; set; }
         string EmpUID { get; set; }
         DateTime? StockConversionDateTime { get; set; }

    }
}
