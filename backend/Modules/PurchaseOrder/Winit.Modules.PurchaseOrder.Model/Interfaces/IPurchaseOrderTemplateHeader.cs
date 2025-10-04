using Winit.Modules.Base.Model;

namespace Winit.Modules.PurchaseOrder.Model.Interfaces;

public interface IPurchaseOrderTemplateHeader : IBaseModel
{
    string OrgUID { get; set; }
    string TemplateName { get; set; }
    string StoreUid { get; set; }
    bool IsCreatedByStore { get; set; }
    bool IsActive { get; set; }
}
