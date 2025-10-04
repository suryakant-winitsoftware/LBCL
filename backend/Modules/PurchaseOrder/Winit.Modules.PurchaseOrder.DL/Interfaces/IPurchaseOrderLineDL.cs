using System.Data;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.DL.Interfaces;

public interface IPurchaseOrderLineDL
{
    Task<int> CreatePurchaseOrderLines(List<IPurchaseOrderLine> purchaseOrderLines, IDbConnection? dbConnection = null,
        IDbTransaction? dbTransaction = null);
    Task<int> UpdatePurchaseOrderLines(List<IPurchaseOrderLine> purchaseOrderLines, IDbConnection? dbConnection = null,
        IDbTransaction? dbTransaction = null);

    Task<int> DeletePurchaseOrderLinesByUIDs(List<string> purchaseOrderLineUIDs, IDbConnection? dbConnection = null,
        IDbTransaction? dbTransaction = null);
    Task<PagedResponse<IPurchaseOrderLine>> GetAllPurchaseOrderLines(List<SortCriteria>? sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria>? filterCriterias, bool isCountRequired);
    Task<int> DeletePurchaseOrderLinesByPurchaseOrderHeaderUID(string purchaseOrderHeaderUID, IDbConnection? dbConnection = null,
        IDbTransaction? dbTransaction = null);

    Task<int> UpdateWallet(List<IPurchaseOrderLine> purchaseOrderLines, string OrgUid, string BranchUId, string HOUID
        , IDbConnection? connection = null, IDbTransaction? transaction = null);
}
