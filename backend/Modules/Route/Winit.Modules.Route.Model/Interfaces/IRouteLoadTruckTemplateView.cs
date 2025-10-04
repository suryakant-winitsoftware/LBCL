using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Route.Model.Interfaces
{
    public interface IRouteLoadTruckTemplateView:IBaseModel
    {
            public IRouteLoadTruckTemplate RouteLoadTruckTemplate { get; set; }
            public List<IRouteLoadTruckTemplateLine>RouteLoadTruckTemplateLineList { get; set; } 
    }
}
