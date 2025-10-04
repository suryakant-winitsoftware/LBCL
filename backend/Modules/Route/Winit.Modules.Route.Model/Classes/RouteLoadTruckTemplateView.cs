using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Route.Model.Classes
{
    public class RouteLoadTruckTemplateView : Winit.Modules.Base.Model.BaseModel, IRouteLoadTruckTemplateView
    {
        public IRouteLoadTruckTemplate RouteLoadTruckTemplate { get; set; }
        public List<IRouteLoadTruckTemplateLine> RouteLoadTruckTemplateLineList { get; set;}
    }

}
