using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Planogram.Model.Interfaces;

namespace Winit.Modules.Planogram.Model.Classes
{
    public class PlanogramExecutionDetail : BaseModel, IPlanogramExecutionDetail
    {
        public string PlanogramExecutionHeaderUID { get; set; } 

        public string PlanogramSetupUID { get; set; }

        public DateTime? ExecutedOn { get; set; }

        public bool? IsCompleted { get; set; } 
        public bool? IsPlanogramAsPerPlan { get; set; } 
    }
}
