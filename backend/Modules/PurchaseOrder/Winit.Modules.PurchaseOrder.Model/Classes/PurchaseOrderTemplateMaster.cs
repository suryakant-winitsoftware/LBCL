using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.Model.Classes;

public class PurchaseOrderTemplateMaster : IPurchaseOrderTemplateMaster
{
    public ActionType ActionType { get; set; }
    public IPurchaseOrderTemplateHeader PurchaseOrderTemplateHeader { get; set; } = new PurchaseOrderTemplateHeader();
    public List<IPurchaseOrderTemplateLine> PurchaseOrderTemplateLines { get; set; } = new();
}
