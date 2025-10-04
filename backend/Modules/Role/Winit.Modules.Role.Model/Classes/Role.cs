using Winit.Modules.Base.Model;
using Winit.Modules.Common.Model.Classes.AuditTrail;
using Winit.Modules.Role.Model.Interfaces;

namespace Winit.Modules.Role.Model.Classes
{
    public class Role : BaseModel, IRole
    {
        [AuditTrail("Name")]
        public string RoleNameEn { get; set; }
        [AuditTrail("Alias Name")]
        public string RoleNameOther { get; set; }
        [AuditTrail]
        public string Code { get; set; }
        public string MenuData { get; set; }
        [AuditTrail("Is Web Role")]
        public bool IsWebUser { get; set; }
        [AuditTrail("Is Active")]
        public bool IsActive { get; set; }
        public string WebMenuData { get; set; }
        public string MobileMenuData { get; set; }
        [AuditTrail("Is App Role")]
        public bool IsAppUser { get; set; }
        public string OrgUid { get; set; }
        public bool BuToDistAccess { get; set; }
        [AuditTrail("Is Principal Role")]
        public bool IsPrincipalRole { get; set; }
        [AuditTrail("Is Distributor Role")]
        public bool IsDistributorRole { get; set; }
        public bool IsAdmin { get; set; }
        public string ParentRoleUid { get; set; }
        [AuditTrail("Parent Role")]
        public string ParentRoleName { get; set; }
        [AuditTrail("Have Warehouse")]
        public bool HaveWarehouse { get; set; }
        [AuditTrail("Have Vehicle")]
        public bool HaveVehicle { get; set; }
        public bool IsBranchApplicable { get; set; }
        [AuditTrail("Is For Reports To")]
        public bool IsForReportsTo { get; set; }
        [AuditTrail("P1")]
        public bool HasP1Access { get; set; }
        [AuditTrail("P2")]
        public bool HasP2Access { get; set; }
        [AuditTrail("P3")]
        public bool HasP3Access { get; set; }
        [AuditTrail("Margin")]
        public bool HasMarginAccess { get; set; }
        [AuditTrail("Minimum Selling Price")]
        public bool HasMSPAccess { get; set; }
    }
}
