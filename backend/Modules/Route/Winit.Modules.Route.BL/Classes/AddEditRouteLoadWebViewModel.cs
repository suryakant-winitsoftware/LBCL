using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Route.BL.Classes
{
    public class AddEditRouteLoadWebViewModel: AddEditRouteLoadBaseViewModel
    {
        public AddEditRouteLoadWebViewModel(IServiceProvider serviceProvider,
    IFilterHelper filter,
    ISortHelper sorter,
    IListHelper listHelper,
    IAppUser appUser,
       IAppConfig appConfigs,
    Base.BL.ApiService apiService
         ):base(serviceProvider, filter, sorter, listHelper, appUser, appConfigs, apiService)
        {

            _filter = filter;
            _sorter = sorter;
            _serviceProvider = serviceProvider;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appUser = appUser;

        }
    }
}
