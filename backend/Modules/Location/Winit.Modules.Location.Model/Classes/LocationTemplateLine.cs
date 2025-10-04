using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Location.Model.Interfaces;

namespace Winit.Modules.Location.Model.Classes
{
    public class LocationTemplateLine:BaseModel, ILocationTemplateLine
    {
        public string LocationTemplateUID { get; set; }
        public string LocationTypeUID { get; set; }
        public string LocationUID { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public bool IsExcluded { get; set; }
        public bool IsSelected { get; set; }
    }
}
