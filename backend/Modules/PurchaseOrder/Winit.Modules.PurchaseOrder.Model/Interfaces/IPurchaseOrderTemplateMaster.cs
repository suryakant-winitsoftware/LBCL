using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.Model.Interfaces;

public interface IPurchaseOrderTemplateMaster
{
    public ActionType ActionType { get; set; }
    public IPurchaseOrderTemplateHeader PurchaseOrderTemplateHeader { get; set; }
    public List<IPurchaseOrderTemplateLine> PurchaseOrderTemplateLines { get; set; }
}
