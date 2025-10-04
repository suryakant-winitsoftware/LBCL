using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Route.Model.Classes
{
    public class RouteScheduleCustomerMapping : BaseModel, IRouteScheduleCustomerMapping
    {
        public string RouteScheduleUID { get; set; }
        public string RouteScheduleConfigUID { get; set; }
        public string CustomerUID { get; set; }
        public int SeqNo { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public bool IsDeleted { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCode { get; set; }
        public string StoreUID { get; set; }
    }
}