using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Tally.Model.Interfaces;

namespace Winit.Modules.Tally.Model.Classes
{
    public class SalesOrderLineFromDB : BaseModel, ISalesOrderLineFromDB
    {
        public string SalesOrderUID { get; set; }
        public string TotalAmountWithDiscount { get; set; }
        public string VOUCHERNUMBER { get; set; }
        public string SERILANUMBER { get; set; }
        public string ItemName { get; set; }
        public string UOM { get; set; }
        public string Type { get; set; }
        public string Quantity { get; set; }
        public string Rate { get; set; }
        public string ItemAmount { get; set; }
        public string DiscountRate { get; set; }
        public string DiscountAmount { get; set; }
        public string TotalTax { get; set; }
        public string TaxUID { get; set; }
    }
}
