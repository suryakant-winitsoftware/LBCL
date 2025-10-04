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
    public class WHStockLedger : BaseModel, IWHStockLedger
    {
        public string CompanyUID { get; set; }
        public string WarehouseUID { get; set; }
        public string OrgUID { get; set; }
        public string SKUUID { get; set; }
        public string SKUCode { get; set; }
        public int Type { get; set; }
        public string ReferenceType { get; set; }
        public string ReferenceUID { get; set; }
        public string BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal? Qty { get; set; }
        public string UOM { get; set; }
        public string StockType { get; set; }
        public string SerialNo { get; set; }
        public string VersionNo { get; set; }
        public string ParentWhUID { get; set; }
        public int YearMonth { get; set; }
    }
}
