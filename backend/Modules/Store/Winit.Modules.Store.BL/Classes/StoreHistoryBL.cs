using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.DL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.BL.Classes;

public class StoreHistoryBL : IStoreHistoryBL
{
    private IStoreHistoryDL _storeHistoryDL { get; }

    public StoreHistoryBL(IStoreHistoryDL storeHistoryDL)
    {
        _storeHistoryDL = storeHistoryDL;
    }
    public async Task<IStoreHistory?> GetStoreHistoryByRouteUIDVisitDateAndStoreUID(string routeUID, string visitDate, string storeUID)
    {
        return await _storeHistoryDL.GetStoreHistoryByRouteUIDVisitDateAndStoreUID(routeUID, visitDate, storeUID);
    }

}
