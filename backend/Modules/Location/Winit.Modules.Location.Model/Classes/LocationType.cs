using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Location.Model.Interfaces;

namespace Winit.Modules.Location.Model.Classes
{
    public class LocationType:BaseModel,ILocationType
    {
        public string CompanyUID { get; set; }
        public string Name { get; set; }
        public string ParentUID { get; set; }
        public string Code { get; set; }
        public int LevelNo { get; set; }
        public bool ShowInUI { get; set; }
        public bool? ShowInTemplate { get; set; }
    }
}
