using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.Models.Events
{
    public class QtyChangeEvent
    {
        public string UID { get; set; }
        public decimal Qty { get; set; }
    }
}
