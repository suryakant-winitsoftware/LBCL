using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Syncing.Model.Interfaces
{
    public interface ISyncLog
    {
        public string MethodName { get; set; }
        public string EmployeeUID { get; set; }
        public string JobPositionUID { get; set; }
        public string RoleUID { get; set; }
        public string VehicleUID { get; set; }
        public DateTime LastSyncTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<ITableSyncDetail> TableSyncDetails { get; set; } 
    }
}
