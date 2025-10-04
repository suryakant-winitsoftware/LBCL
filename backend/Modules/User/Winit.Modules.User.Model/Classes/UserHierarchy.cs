using Winit.Modules.User.Model.Interface;

namespace Winit.Modules.User.Model.Classes;

public class UserHierarchy : IUserHierarchy
{
    public string RoleCode { get; set; } = string.Empty;
    public string EmpUID { get; set; } = string.Empty;
    public string EmpCode { get; set; } = string.Empty;
    public string? EmpName { get; set; }
    public int LevelNo { get; set; }
    public int ApprovalLevel { get; set; }
}
