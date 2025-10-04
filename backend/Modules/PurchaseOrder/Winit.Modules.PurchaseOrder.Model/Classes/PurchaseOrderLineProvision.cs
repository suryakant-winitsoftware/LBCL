using Winit.Modules.Base.Model;
using Winit.Modules.Common.Model.Classes.AuditTrail;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
namespace Winit.Modules.PurchaseOrder.Model.Classes;

public class PurchaseOrderLineProvision : BaseModel, IPurchaseOrderLineProvision
{
    [AuditTrail]
    public string PurchaseOrderLineUID { get; set; }
    [AuditTrail]
    public string ProvisionType { get; set; }
    [AuditTrail]
    public string SchemeCode { get; set; }
    public decimal ActualProvisionUnitAmount { get; set; }
    [AuditTrail("Provision Amount")]
    public decimal ApprovedProvisionUnitAmount { get; set; }
    [AuditTrail]
    public string Remarks { get; set; }
    public PurchaseOrderLineProvision()
    {
        IsSelected = true;
    }
}
