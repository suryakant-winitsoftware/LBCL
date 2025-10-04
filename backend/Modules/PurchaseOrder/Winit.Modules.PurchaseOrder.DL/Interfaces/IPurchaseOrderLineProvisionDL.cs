using System.Data;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
namespace Winit.Modules.PurchaseOrder.DL.Interfaces;

public interface IPurchaseOrderLineProvisionDL
{
    Task<PagedResponse<IPurchaseOrderLineProvision>> GetAllPurchaseOrderLineProvisions(List<FilterCriteria>? filters = null, List<SortCriteria>? sorts = null,
        int? pageSize = null, int? pageNumber = null, bool isCountRequired = false);
    Task<int> CreatePurchaseOrderLineProvisions(List<IPurchaseOrderLineProvision> purchaseOrderLineProvisions, IDbConnection dbConnection = null,
        IDbTransaction dbTransaction = null);
    Task<int> UpdatePurchaseOrderLineProvisions(List<IPurchaseOrderLineProvision> purchaseOrderLineProvisions, IDbConnection dbConnection = null,
        IDbTransaction dbTransaction = null);
    Task<int> DeletePurchaseOrderLineProvisionsByUids(List<string> purchaseOrderLineProvisionUiDs, IDbConnection dbConnection = null,
        IDbTransaction dbTransaction = null);
    Task<int> DeletePurchaseOrderLineProvisionsByPurchaseOrderLineUids(List<string> purchaseOrderLineProvisionUID, IDbConnection dbConnection = null,
        IDbTransaction dbTransaction = null);
    Task<List<string>?> GetPurchaseOrderLineProvisionUidsByPurchaseOrderUID(string purchaseOrderUID, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        return null;
    }
}
