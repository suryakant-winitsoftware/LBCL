using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.PurchaseOrder.Model.Classes;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.Model.Interfaces;

public interface IPurchaseOrderMaster
{
    public ActionType ActionType { get; set; }
    IPurchaseOrderHeader? PurchaseOrderHeader { get; set; }
    List<IPurchaseOrderLine>? PurchaseOrderLines { get; set; }
    List<IPurchaseOrderLineProvision> PurchaseOrderLineProvisions { get; set; }
    ApprovalStatusUpdate ApprovalStatusUpdate { get; set; }
    ApprovalRequestItem ApprovalRequestItem { get; set; }
}
