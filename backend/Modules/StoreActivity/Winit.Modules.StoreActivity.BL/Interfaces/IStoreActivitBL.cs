using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.StoreActivity.Model.Interfaces;

namespace Winit.Modules.StoreActivity.BL.Interfaces;

public  interface IStoreActivitBL
{
    Task<IEnumerable<IStoreActivityItem>> GetAllStoreActivities(string RoleUID, string StoreHistoryUID);
    Task<int> UpdateStoreActivityHistoryStatus(string storeActivityHistoryUID, string status);
}
