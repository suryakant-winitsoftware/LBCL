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
    public class WHStockConversion : BaseModel, IWHStockConversion
    {
       public string CompanyUID { get; set; }
       public string WarehouseUID { get; set; }
       public string OrgUID { get; set; }
       public string Status { get; set; }
       public string JobPositionUID { get; set; }
       public string EmpUID { get; set; }
       public DateTime? StockConversionDateTime { get; set; }
    }
}
