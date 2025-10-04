using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Route.Model.Classes
{
    public class RouteLoadTruckTemplateUI: Winit.Modules.Base.Model.BaseModel, Winit.Modules.Route.Model.Interfaces.IRouteLoadTruckTemplateUI
    {
        public string RouteUID { get; set; }
        public string TemplateName { get; set; }
        public string TemplateDescription { get; set; }
        public string CompanyUID { get; set; }
        public string OrgUID { get; set; }
        public bool IsSelected { get; set; } = false;
    }
}
