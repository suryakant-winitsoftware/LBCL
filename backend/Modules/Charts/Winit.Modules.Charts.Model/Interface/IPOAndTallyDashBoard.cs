using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Chart.Models.Interfaces
{
    public interface IPOAndTallyDashBoard 
    {
       public int Year { get; set; }
        public int Month { get; set; }
        public string? Primary {  get; set; }
        public string? Secondary { get; set; }
    }
}
