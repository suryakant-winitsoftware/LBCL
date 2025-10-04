using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Syncing.Model.Interfaces
{
    public interface ISyncRequest
    {
        public string GroupName { get; set; }
        public string TableName { get; set; }
        public string EmployeeUID { get; set; }
        public string EmployeeCode { get; set; }
        public string JobPositionUID { get; set; }
        public string RoleUID { get; set; }
        public string VehicleUID { get; set; }
        public DateTime LastSyncTime { get; set; }
        public string OrgUID { get; set; }
    }
}
