using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Location.Model.Interfaces;

namespace Winit.Modules.Location.Model.Classes
{
    public class LocationMapping:BaseModel,ILocationMapping
    {
        public string LinkedItemUID { get; set; }
        public string LinkedItemType { get; set; }
        public string LocationTypeUID { get; set; }
        public string LocationUID { get; set; }
    }
}
