using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Mobile.Model.Interfaces;

namespace Winit.Modules.Mobile.Model.Classes
{
    public class SqlitePreparation : BaseModel, ISqlitePreparation
    {
        public string EmpUID { get; set; }
        public string JobPosition_UID { get; set; }
        public string RoleUID { get; set; }
        public string Status { get; set; }
        public string SqlitePath { get; set; }
        public string ErrorMessage { get; set; }
        public string VehicleUID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

    }
}
