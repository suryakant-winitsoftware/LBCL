using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StockUpdater.Model.Interfaces
{
    public interface IWHStockLedger : IBaseModel
    {
         string? CompanyUID { get; set; }
         string WarehouseUID { get; set; }
         string OrgUID { get; set; }
         string SKUUID { get; set; }
         string SKUCode { get; set; }
         int Type { get; set; }
         string ReferenceType { get; set; }
         string ReferenceUID { get; set; }
         string BatchNumber { get; set; }
         DateTime? ExpiryDate { get; set; }
         decimal Qty { get; set; }
         string UOM { get; set; }
         string StockType { get; set; }
         string? SerialNo { get; set; }
         string VersionNo { get; set; }
         string ParentWhUID { get; set; }
         int YearMonth { get; set; }

    }
}
