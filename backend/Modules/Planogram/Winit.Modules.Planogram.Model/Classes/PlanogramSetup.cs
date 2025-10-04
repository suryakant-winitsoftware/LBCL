using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Planogram.Model.Interfaces;

namespace Winit.Modules.Planogram.Model.Classes
{
    public class PlanogramSetup : BaseModel , IPlanogramSetup
    {
        public string CategoryCode { get; set; } 

        public decimal? ShareOfShelfCm { get; set; }

        public string SelectionType { get; set; } 

        public string SelectionValue { get; set; } 
    }
}
