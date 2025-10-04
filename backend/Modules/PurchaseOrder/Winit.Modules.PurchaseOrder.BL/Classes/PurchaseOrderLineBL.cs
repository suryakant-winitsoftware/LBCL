using System.Data;
using Winit.Modules.PurchaseOrder.BL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.BL.Classes;

public class PurchaseOrderLineBL : IPurchaseOrderLineBL
{
    protected readonly DL.Interfaces.IPurchaseOrderLineDL _purchaseOrderLineDL;
    private readonly IServiceProvider _serviceProvider;
    public PurchaseOrderLineBL(DL.Interfaces.IPurchaseOrderLineDL purchaseOrderLineDL, IServiceProvider serviceProvider)
    {
        _purchaseOrderLineDL = purchaseOrderLineDL;
        _serviceProvider = serviceProvider;
    }


    public async Task<int> DeletePurchaseOrderLinesByUIDs(List<string> purchaseOrderLineUIDs)
    {
        return await _purchaseOrderLineDL.DeletePurchaseOrderLinesByUIDs(purchaseOrderLineUIDs);
    }
}
