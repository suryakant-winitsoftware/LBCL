using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Chart.Models.Interfaces;

namespace Winit.Modules.Chart.Models.Classes
{
    public class POAndTallyDashBoard : IPOAndTallyDashBoard
    {
       public int Year { get; set; }
        public int Month { get; set; }
        public string? Primary {  get; set; }
        public string? Secondary { get; set; }
    }
}
