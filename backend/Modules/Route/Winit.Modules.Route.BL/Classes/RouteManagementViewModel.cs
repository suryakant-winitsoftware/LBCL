using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Route.BL.Classes;

public class RouteManagementViewModel : RouteManagementBaseViewModel
{
    public RouteManagementViewModel(IServiceProvider serviceProvider,
                                    IFilterHelper filter,
                                    ISortHelper sorter,
                                    IListHelper listHelper,
                                    IAppUser appUser,
                                    IAppConfig appConfigs,
                                    Base.BL.ApiService apiService
        ) : base(serviceProvider, filter, sorter, listHelper, appUser, appConfigs, apiService)
    { }
}
