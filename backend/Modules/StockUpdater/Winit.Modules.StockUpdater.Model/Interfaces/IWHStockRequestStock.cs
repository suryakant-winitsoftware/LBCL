using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StockUpdater.Model.Interfaces
{
    public interface IWHStockRequestStock : IBaseModelV2
    {

        public string WhStockRequestUID { get; set; }
        public string WhStockRequestLineUID { get; set; }
        public string BatchNumber { get; set; }
        public string ExpiryDate { get; set; }
        public string SerialNo { get; set; }
        public string VersionNo { get; set; }
        public decimal? Qty { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalLineTax { get; set; }
        public decimal? TotalHeaderTax { get; set; }
        public decimal? TotalTax { get; set; }
        public decimal? NetAmount { get; set; }
        public string OrgUID { get; set; }
        public string WareHouseUID { get; set; }
        public int YearMonth { get; set; }
    }
}
