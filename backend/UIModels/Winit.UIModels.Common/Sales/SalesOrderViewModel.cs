using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SalesOrder.Model.Classes;

namespace Winit.UIModels.Common.Sales
{
    public class SalesOrderViewModel
    {
        public SalesOrder SalesOrder { get; set; }
        public List<SalesOrderLine> SalesOrderLine { get; set; }
    }
}
