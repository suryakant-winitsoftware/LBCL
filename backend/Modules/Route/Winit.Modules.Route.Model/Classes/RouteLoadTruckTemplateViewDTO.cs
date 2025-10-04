using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Route.Model.Classes
{
    public class RouteLoadTruckTemplateViewDTO
    { 
        public RouteLoadTruckTemplate RouteLoadTruckTemplate { get; set; }
        public List<RouteLoadTruckTemplateLine> RouteLoadTruckTemplateLineList { get; set; }
    }

}
