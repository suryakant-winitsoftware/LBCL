using System.Collections.Generic;
namespace Winit.Modules.ApprovalEngine.Model.Classes;

public class ApprovalRequestItem
{
    public Dictionary<string, object> Payload { get; set; }
    public string HierarchyUid { get; set; }
    public string HierarchyType { get; set; }
    public int RuleId { get; set; }
    public string RequestId { get; set; }
}
