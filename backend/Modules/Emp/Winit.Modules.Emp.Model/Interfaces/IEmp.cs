using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Emp.Model.Interfaces
{
    public interface IEmp : IBaseModel
    {
        public string CompanyUID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string AliasName { get; set; }
        public string LoginId { get; set; }
        public string EmpNo { get; set; }
        public string AuthType { get; set; }
        public string Status { get; set; }
        public string ActiveAuthKey { get; set; }
        public string EncryptedPassword { get; set; }
        public ActionType ActionType { get; set; }
        public string ProfileImage { get; set; }
        public string? JobPositionUid { get; set; }
        public string? ApprovalStatus { get; set; }
        public ApprovalStatusUpdate ApprovalStatusUpdate { get; set; }
        public bool IsMandatoryChangePassword { get; set; }
    }
}
