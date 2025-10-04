using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Route.Model.Interfaces
{
    public interface IRouteLoadTruckTemplateUI: IRouteLoadTruckTemplate, Winit.Modules.Base.Model.IBaseModel
    {
      
        public bool IsSelected { get; set; }
    }
}
