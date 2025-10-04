using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;

namespace Winit.Modules.Route.BL.Classes
{
    public class RouteLoadWebViewModel : RouteLoadBaseViewModel
    {
      
        public RouteLoadWebViewModel(IServiceProvider serviceProvider,
      IFilterHelper filter,
      ISortHelper sorter,
      IListHelper listHelper,
      IAppUser appUser,

      IAppConfig appConfigs,
      Base.BL.ApiService apiService
  ) : base(serviceProvider, filter, sorter, listHelper, appUser, appConfigs, apiService)
        {

            _filter = filter;
            _sorter = sorter;
            _serviceProvider = serviceProvider;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appUser = appUser;
            FilterCriterias = new List<Winit.Shared.Models.Enums.FilterCriteria>();

        }
    }
}
