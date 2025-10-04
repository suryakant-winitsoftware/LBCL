namespace Winit.Modules.ApprovalEngine.Model.Classes;

public class ApprovalStatusUpdate
{
    public string Status { get; set; }
    public string Remarks { get; set; }
    public string RequesterId { get; set; }
    public string RoleCode { get; set; }
    public bool IsFinalApproval { get; set; }
    public string RequestId { get; set; }
}
