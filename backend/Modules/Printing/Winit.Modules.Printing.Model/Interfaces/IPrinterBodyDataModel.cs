using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Printing.Model.Interfaces
{
    public interface IPrinterBodyDataModel
    {
        public int SNo { get; set; }
        public string Description { get; set; }
        public string UOM { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
