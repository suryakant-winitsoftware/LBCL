using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Org.Model.Interfaces
{
    public interface IFOCStockItem
    {
         public string ReservedOUQty { get; set; }
         public string ReservedBUQty { get; set; }
         public string ItemUID { get; set; }
    }
}
