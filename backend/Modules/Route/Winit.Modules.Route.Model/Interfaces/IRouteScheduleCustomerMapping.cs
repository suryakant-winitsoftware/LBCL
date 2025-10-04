using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Route.Model.Interfaces
{
    public interface IRouteScheduleCustomerMapping : IBaseModel
    {
        string RouteScheduleUID { get; set; }
        string RouteScheduleConfigUID { get; set; }
        string CustomerUID { get; set; }
        int SeqNo { get; set; }
        string StartTime { get; set; }
        string EndTime { get; set; }
        bool IsDeleted { get; set; }
        string CustomerName { get; set; }
        string CustomerCode { get; set; }
        string StoreUID { get; set; }
    }
}