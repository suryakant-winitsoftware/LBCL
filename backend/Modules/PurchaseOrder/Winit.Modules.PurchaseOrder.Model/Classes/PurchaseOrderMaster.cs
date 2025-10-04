using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.Models.Enums;
using Winit.Modules.Common.Model.Classes.AuditTrail;

namespace Winit.Modules.PurchaseOrder.Model.Classes;

public class PurchaseOrderMaster : IPurchaseOrderMaster
{
    public ActionType ActionType { get; set; }
    public IPurchaseOrderHeader? PurchaseOrderHeader { get; set; }
    public List<IPurchaseOrderLine>? PurchaseOrderLines { get; set; }
    public List<IPurchaseOrderLineProvision> PurchaseOrderLineProvisions { get; set; } = new List<IPurchaseOrderLineProvision>();
    
    public ApprovalStatusUpdate ApprovalStatusUpdate { get; set; }
    public ApprovalRequestItem ApprovalRequestItem { get; set; }
}
