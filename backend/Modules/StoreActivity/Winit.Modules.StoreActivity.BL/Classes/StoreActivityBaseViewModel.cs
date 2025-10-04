using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.StoreActivity.BL.Interfaces;
using Winit.Modules.StoreActivity.Model.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Role.Model.Interfaces;
namespace Winit.Modules.StoreActivity.BL.Classes;

public class StoreActivityBaseViewModel:IStoreActivityViewModel
{
    public IStoreActivitBL _storeActivitBL { get; set; }
    
    private IServiceProvider _serviceProvider;
    private IFilterHelper _filter;
    private ISortHelper _sorter;
    private IListHelper _listHelper;
    private IAppUser _appUser;
    private IAppConfig _appConfigs;
    private ApiService _apiService;
    public StoreActivityBaseViewModel(IServiceProvider serviceProvider, IFilterHelper filter, ISortHelper sorter,
            IListHelper listHelper, IAppUser appUser, IAppConfig appConfigs, ApiService apiService, IStoreActivitBL storeActivitBL)
    {
        this._serviceProvider = serviceProvider;
        this._filter = filter;
        this._sorter = sorter;
        this._listHelper = listHelper;
        this._appUser = appUser;
        this._appConfigs = appConfigs;
        this._apiService = apiService;
        this._storeActivitBL = storeActivitBL;
       
    }
    public IRole RoleUID { get; set; }
    public List<IStoreActivityItem> storeActivityItems { get; set; }

    public async Task<IEnumerable<IStoreActivityItem>> GetAllStoreActivities(string RoleUID, string StoreHistoryUID)
    {
        return await _storeActivitBL.GetAllStoreActivities(RoleUID, StoreHistoryUID);
    }
    public async Task<int> UpdateStoreActivityHistoryStatus(string storeActivityHistoryUID, string status)
    {
        return await _storeActivitBL.UpdateStoreActivityHistoryStatus(storeActivityHistoryUID, status);
    }
}
