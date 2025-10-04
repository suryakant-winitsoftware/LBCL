using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Org.Model.Interfaces;

namespace Winit.Modules.Org.Model.Classes
{
    public class FOCStockItem : IFOCStockItem
    {
        public string ReservedOUQty { get; set; }
        public string ReservedBUQty { get; set; }
        public string ItemUID { get; set; }
    }
}

