using Winit.Modules.Base.Model;
using Winit.Modules.PurchaseOrder.Model.Interfaces;

namespace Winit.Modules.PurchaseOrder.Model.Classes;

public class PurchaseOrderTemplateHeader : BaseModel, IPurchaseOrderTemplateHeader
{
    public string OrgUID { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string? StoreUid { get; set; }
    public bool IsCreatedByStore { get; set; }
    public bool IsActive { get; set; }
}
