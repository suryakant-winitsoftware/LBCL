using Winit.Modules.Base.Model;
using Winit.Modules.Common.Model.Classes.AuditTrail;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Emp.Model.Classes
{
    public class EmpInfo : BaseModel, IEmpInfo
    {
        public string EmpUID { get; set; }
        [AuditTrail]
        public string Email { get; set; }
        [AuditTrail]
        public string Phone { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool CanHandleStock { get; set; }
        public string ADGroup { get; set; }
        public string ADUsername { get; set; }
        public ActionType ActionType { get; set; }
    }
}
