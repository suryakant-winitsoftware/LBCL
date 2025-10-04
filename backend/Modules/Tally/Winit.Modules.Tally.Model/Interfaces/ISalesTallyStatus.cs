using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Tally.Model.Interfaces
{
    public interface ISalesTallyStatus
    {
        public string SalesOrderNumber { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public string VoucherId { get; set; }
    }
}
