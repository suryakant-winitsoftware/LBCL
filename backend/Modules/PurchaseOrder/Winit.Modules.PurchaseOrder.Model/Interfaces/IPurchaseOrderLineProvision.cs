using Winit.Modules.Base.Model;
using Winit.Modules.Common.Model.Classes.AuditTrail;
namespace Winit.Modules.PurchaseOrder.Model.Interfaces;

public interface IPurchaseOrderLineProvision : IBaseModel
{
    string PurchaseOrderLineUID { get; set; }
    string ProvisionType { get; set; }
    string SchemeCode { get; set; }
    decimal ActualProvisionUnitAmount { get; set; }
    decimal ApprovedProvisionUnitAmount { get; set; }
    string Remarks { get; set; }
}
