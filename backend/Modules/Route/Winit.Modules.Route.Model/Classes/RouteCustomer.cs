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
    public class RouteCustomer:BaseModel, IRouteCustomer
    {
        public string RouteUID { get; set; }
        public string StoreUID { get; set; }
        public int SeqNo { get; set; }
        public string VisitTime { get; set; }
        public int VisitDuration { get; set; }
        public string EndTime { get; set; }
        public bool IsDeleted { get; set; }
        public int TravelTime { get; set; }
        public ActionType ActionType { get; set; }
        public string Frequency { get; set; }
        public string StoreName { get; set; }
        public string StoreCode { get; set; }
        public string StoreAliasName { get; set; }
        public string StoreCity { get; set; }
        public string StoreRegion { get; set; }
    }
}
