namespace Winit.Modules.User.Model.Interface;

public interface IUserHierarchy
{
    public string RoleCode { get; set; }
    public string EmpUID { get; set; }
    public string EmpCode { get; set; }
    public string? EmpName { get; set; }
    public int LevelNo { get; set; }
    public int ApprovalLevel { get; set; }
}
