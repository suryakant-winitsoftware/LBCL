using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Route.Model.Interfaces;

namespace Winit.Modules.Route.Model.Classes
{
    public class RouteLoadTruckTemplateViewModel
    { 
        public RouteLoadTruckTemplate RouteLoadTruckTemplate { get; set; }
        public List<RouteLoadTruckTemplateLine> RouteLoadTruckTemplateLineList { get; set; }
    }
}
