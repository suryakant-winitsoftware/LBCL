using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SalesOrder.Model.UIInterfaces;

namespace Winit.Modules.SalesOrder.Model.Interfaces
{
    public interface ISKUViewPreSales
    {
        public string? SKUCode { get; set; }
        public string? SKUName { get; set; }
        public string? ItemType { get; set; }
        public string? UoM { get; set; }
        public decimal? RecoQty { get; set; }
        public decimal? Qty { get; set; }
        public decimal? DeliveredQty { get; set; }
        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }
        public decimal? AmountExcTax { get; set; }
        public decimal? Tax { get; set; }
        public decimal? AmountIncTax { get; set; }
        public decimal? ApprovedQty { get; set; }
        


    }
  
}
