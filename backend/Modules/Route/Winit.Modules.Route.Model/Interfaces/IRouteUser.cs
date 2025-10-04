using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Route.Model.Interfaces
{
    public interface IRouteUser:IBaseModel
    {
        public string RouteUID { get; set; }
        public string JobPositionUID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool IsActive { get; set; }
        public ActionType ActionType { get; set; }
    }
}
