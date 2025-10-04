using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Route.Model.Interfaces
{
    public interface IRoute : Winit.Modules.Base.Model.IBaseModel
    {
        public string CompanyUID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string OrgUID { get; set; }
        public string WHOrgUID { get; set; }
        public string VehicleUID { get; set; }
        public string JobPositionUID { get; set; }
        public string LocationUID { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUpto { get; set; }
        public bool PrintStanding { get; set; }
        public bool PrintForward { get; set; }
        public bool PrintTopup { get; set; }
        public bool PrintOrderSummary { get; set; }
        public bool AutoFreezeJP { get; set; }
        public bool AddToRun { get; set; }
        public string AutoFreezeRunTime { get; set; }
        public int TotalCustomers { get; set; }
        public string VisitTime { get; set; }
        public string EndTime { get; set; }
        public int VisitDuration { get; set; }
        public int TravelTime { get; set; }
        public bool IsCustomerWithTime{ get; set; }
        public string RoleUID { get; set; }
    }
}
