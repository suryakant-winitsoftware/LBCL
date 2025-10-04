using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.StoreActivity.BL.Interfaces;
using Winit.Modules.StoreActivity.DL.Interfaces;
using Winit.Modules.StoreActivity.Model.Interfaces;

namespace Winit.Modules.StoreActivity.BL.Classes;

public class StoreActivityBL : IStoreActivitBL
{
    public IStoreActivitDL _storeActivitDL { get; set; }
    public StoreActivityBL(IStoreActivitDL storeActivitDL)
    {
        _storeActivitDL = storeActivitDL;   
    }

    public async Task<IEnumerable<IStoreActivityItem>> GetAllStoreActivities(string RoleUID, string StoreHistoryUID)
    {
        return await _storeActivitDL.GetAllStoreActivities(RoleUID, StoreHistoryUID);
    }

    public async Task<int> UpdateStoreActivityHistoryStatus(string storeActivityHistoryUID, string status)
    {
        return await _storeActivitDL.UpdateStoreActivityHistoryStatus(storeActivityHistoryUID,status);
    }
}
