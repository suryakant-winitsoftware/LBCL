using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Planogram.Model.Interfaces;

namespace Winit.Modules.Planogram.Model.Classes
{
    public class PlanogramCategory : IPlanogramCategory
    {
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public int SetupCount { get; set; }
        public string CategoryImage { get; set; }
    }
}
