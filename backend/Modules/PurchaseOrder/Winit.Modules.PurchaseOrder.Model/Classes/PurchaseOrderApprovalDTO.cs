using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
namespace Winit.Modules.PurchaseOrder.Model.Classes;

public class PurchaseOrderApprovalDTO
{
    public List<IPurchaseOrderMaster> PurchaseOrderMasters { get; set; }
}
