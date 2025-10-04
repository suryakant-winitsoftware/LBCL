using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;

namespace Winit.Modules.SKU.BL.Classes
{
    public class ProductSequencingWebViewModel: ProductSequencingBaseViewModel
    {
        public ProductSequencingWebViewModel(IServiceProvider serviceProvider,
      IFilterHelper filter,
      ISortHelper sorter,
      IListHelper listHelper,
      IAppUser appUser,

      Winit.Shared.Models.Common.IAppConfig appConfigs,
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
            FilterCriterias = new List<Winit.Shared.Models.Enums.FilterCriteria>();

        }
    }
}
