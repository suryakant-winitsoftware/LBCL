using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Tally.Model.Interfaces;

namespace Winit.Modules.Tally.Model.Classes
{
    public class SalesTallyStatus : ISalesTallyStatus 
    { 
        public string SalesOrderNumber { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public string VoucherId { get; set; }
    }
}
