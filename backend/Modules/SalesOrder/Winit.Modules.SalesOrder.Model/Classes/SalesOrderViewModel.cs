using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SalesOrder.Model.Interfaces;

namespace Winit.Modules.SalesOrder.Model.Classes
{
    public class SalesOrderViewModel : ISalesOrderViewModel
    {
        public ISalesOrder SalesOrder { get; set; }
        public List<ISalesOrderLine> SalesOrderLine { get; set; }
    }
}
