using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Mobile.Model.Interfaces
{
    public interface ISqlitePreparation : IBaseModel
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
