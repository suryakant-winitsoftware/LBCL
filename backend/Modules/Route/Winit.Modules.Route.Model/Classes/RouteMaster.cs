using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Route.Model.Classes
{
    public class RouteMaster 
    {
        public RouteChangeLog Route { get; set; }
        public RouteSchedule RouteSchedule { get; set; }
        public List<RouteScheduleConfig> RouteScheduleConfigs { get; set; }
        public List<RouteScheduleCustomerMapping> RouteScheduleCustomerMappings { get; set; }
        public List<RouteCustomer> RouteCustomersList { get; set; }
        public List<RouteUser>? RouteUserList { get; set; }
       

    }

}
