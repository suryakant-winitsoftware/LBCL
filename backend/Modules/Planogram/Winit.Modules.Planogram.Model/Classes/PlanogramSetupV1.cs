using System;
using Winit.Modules.Base.Model;
using Winit.Modules.Planogram.Model.Interfaces;

namespace Winit.Modules.Planogram.Model.Classes
{
    public class PlanogramSetupV1 : BaseModel, IPlanogramSetupV1
    {
        public string Description { get; set; }
        public string ImageUrl { get; set; }
    }
} 