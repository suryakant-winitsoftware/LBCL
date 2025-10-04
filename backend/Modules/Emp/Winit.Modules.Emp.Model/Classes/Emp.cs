using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.Base.Model;
using Winit.Modules.Common.Model.Classes.AuditTrail;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Emp.Model.Classes
{
    public class Emp : BaseModel, IEmp
    {
        public string CompanyUID { get; set; }
        [AuditTrail("Employee Code")]
        public string Code { get; set; }
        [AuditTrail]
        public string Name { get; set; }
        [AuditTrail("Alias Name")]
        public string AliasName { get; set; }
        [AuditTrail("Login ID")]
        public string LoginId { get; set; }
        [AuditTrail("Employee No")]
        public string EmpNo { get; set; }
        [AuditTrail("Auth Type")]
        public string AuthType { get; set; }
        [AuditTrail("Is Active")]
        public string Status { get; set; }
        public string ActiveAuthKey { get; set; }
        public string EncryptedPassword { get; set; }
        public ActionType ActionType { get; set; }
        public string ProfileImage { get; set; }
        public string? JobPositionUid { get; set; }
        public string? ApprovalStatus { get; set; } = "Approved";
        public ApprovalStatusUpdate ApprovalStatusUpdate { get; set; }
        public bool IsMandatoryChangePassword { get; set; }
    }
}
