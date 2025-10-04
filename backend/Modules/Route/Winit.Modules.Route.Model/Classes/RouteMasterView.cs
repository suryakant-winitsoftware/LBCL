using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Route.Model.Classes
{
    public class RouteMasterView :IRouteMasterView
    {
        public IRouteChangeLog Route { get; set; }
        public IRouteSchedule RouteSchedule { get; set; }
        public List<IRouteScheduleConfig> RouteScheduleConfigs { get; set; }
        public List<IRouteScheduleCustomerMapping> RouteScheduleCustomerMappings { get; set; }
        public List<IRouteCustomer> RouteCustomersList { get; set; }
        public List<IRouteUser> RouteUserList { get; set; }
    }

}
