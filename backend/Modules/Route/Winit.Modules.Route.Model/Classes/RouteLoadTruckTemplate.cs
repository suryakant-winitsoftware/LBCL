using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Route.Model.Classes
{
    public class RouteLoadTruckTemplate : BaseModel, IRouteLoadTruckTemplate
    {
        public string RouteUID { get; set; }
        public string TemplateName { get; set; }
        public string TemplateDescription { get; set; }
        public string CompanyUID { get; set; }
        public string OrgUID { get; set; }
       
    }

}
