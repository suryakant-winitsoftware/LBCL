using Winit.Modules.Base.Model;
using Winit.Modules.Common.Model.Classes.AuditTrail;

namespace Winit.Modules.Role.Model.Interfaces
{
    public interface IRole : IBaseModel
    {
        [AuditTrail("Role Name")]
        string RoleNameEn { get; set; }
        [AuditTrail("Alias Name")]
        string RoleNameOther { get; set; }
        [AuditTrail]
        string Code { get; set; }
        string MenuData { get; set; }
        [AuditTrail("Is Web Role")]

        bool IsWebUser { get; set; }
        [AuditTrail("Is Active")]
        bool IsActive { get; set; }
        string WebMenuData { get; set; }
        string MobileMenuData { get; set; }
        [AuditTrail("Is App Role")]

        bool IsAppUser { get; set; }
        string OrgUid { get; set; }
        bool BuToDistAccess { get; set; }
        [AuditTrail("Is Principal Role")]

        bool IsPrincipalRole { get; set; }
        [AuditTrail("Is Distributor Role")]

        bool IsDistributorRole { get; set; }
        bool IsAdmin { get; set; }
        [AuditTrail("Parent Role")]
        string ParentRoleUid { get; set; }
        string ParentRoleName { get; set; }
        [AuditTrail("Have Warehouse")]

        bool HaveWarehouse { get; set; }
        [AuditTrail("Have Vehicle")]

        bool HaveVehicle { get; set; }
        bool IsBranchApplicable { get; set; }
        [AuditTrail("Is For Reports To")]

        bool IsForReportsTo { get; set; }
        [AuditTrail("P1")]

        bool HasP1Access { get; set; }
        [AuditTrail("P2")]
        bool HasP2Access { get; set; }
        [AuditTrail("P3")]
        bool HasP3Access { get; set; }
        [AuditTrail("Margin")]
        bool HasMarginAccess { get; set; }
        [AuditTrail("Minimum Selling Price")]
        bool HasMSPAccess { get; set; }
    }
}
