using System;
using Winit.Modules.Base.Model;
using Winit.Modules.Merchandiser.Model.Interfaces;

namespace Winit.Modules.Merchandiser.Model.Classes
{
    public class ROTAActivity : BaseModel, IROTAActivity
    {
        public string JobPositionUID { get; set; }
        public DateTime RotaDate { get; set; }
        public string RotaActivityName { get; set; }
    }
} 