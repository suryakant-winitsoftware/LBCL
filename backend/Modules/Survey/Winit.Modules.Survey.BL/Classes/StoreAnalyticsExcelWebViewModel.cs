using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Modules.User.Model.Interfaces;
using iTextSharp.text;
using Newtonsoft.Json;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Modules.Survey.Model.Classes;
using Nest;

namespace Winit.Modules.Survey.BL.Classes
{
    public class StoreAnalyticsExcelWebViewModel : StoreAnalyticsExcelBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        IAppUser _appUser;
        public StoreAnalyticsExcelWebViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
        ISortHelper sorter,
            IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService,
            IAppUser appUser) : base(serviceProvider, filter, sorter, listHelper, appConfigs, apiService, appUser)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appUser = appUser;
        }
        public override async Task<int> SaveStoreRollingStats(List<IStoreRollingStatsModel> storeRollingStatsModelList)
        {
            return await SaveStoreRollingStatsFromAPIAsync(storeRollingStatsModelList);
             
        }
        private async Task<int> SaveStoreRollingStatsFromAPIAsync(List<IStoreRollingStatsModel> storeRollingStatsModelList)
        {
            try
            {
                var apiResponse = await _apiService.FetchDataAsync<int>(
                    $"{_appConfigs.ApiBaseUrl}UserStoreActivity/CreateStoreRollingStats",
                    HttpMethod.Post,
                    storeRollingStatsModelList
                );

                return apiResponse?.Data ?? -1; 
            }
            catch (Exception ex)
            {
                throw;
            }
        }





    }
}
