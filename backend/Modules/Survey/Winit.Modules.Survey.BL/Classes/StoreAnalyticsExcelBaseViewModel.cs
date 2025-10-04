using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Classes;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Survey.BL.Interfaces;
using Winit.Shared.Models.Enums;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Modules.Survey.Model.Classes;
using Winit.Modules.Contact.Model.Interfaces;
using Winit.Modules.Contact.Model.Classes;
using Winit.Shared.CommonUtilities.Extensions;

namespace Winit.Modules.Survey.BL.Classes
{
    public abstract class StoreAnalyticsExcelBaseViewModel : IStoreAnalyticsExcelViewModel
    {
        public int PageNumber { get; set; } = 1;
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public List<Winit.Modules.Survey.Model.Classes.StoreRollingStatsModel> ExcelStoreRollingStats { get; set; }
        public List<Winit.Modules.Survey.Model.Interfaces.IStoreRollingStatsModel> storeRollingStatsList { get; set; }
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        protected Winit.Modules.Survey.Model.Interfaces.IStoreRollingStatsModel storeRollingStatsModel;
        IAppUser _appUser;
        public PagingRequest PagingRequest { get; set; } = new PagingRequest()
        {
            FilterCriterias = [],
            SortCriterias = [],
            IsCountRequired = true
        };

        public StoreAnalyticsExcelBaseViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService,
            IAppUser appUser)
        {
            _serviceProvider = serviceProvider;
            _filter = filter; 
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appUser = appUser;
            ExcelStoreRollingStats = new List<Winit.Modules.Survey.Model.Classes.StoreRollingStatsModel>();
            storeRollingStatsModel = new StoreRollingStatsModel();
            if (storeRollingStatsList == null)
                storeRollingStatsList = new List<IStoreRollingStatsModel>();
        }

        public async Task InsertStoreRollingStats()
        {
            if (ExcelStoreRollingStats != null && ExcelStoreRollingStats.Any())
            {
                storeRollingStatsList.Clear();

                foreach (var storeRollingStats in ExcelStoreRollingStats)
                {
                    var newStoreRollingStatsModel = new StoreRollingStatsModel
                    {
                        UID = Guid.NewGuid().ToString(),
                        CreatedBy = _appUser.Emp.UID,
                        CreatedTime = DateTime.Now,
                        ModifiedBy = _appUser.Emp.UID,
                        ModifiedTime = DateTime.Now,
                        ServerAddTime = DateTime.Now,
                        ServerModifiedTime = DateTime.Now,
                        StoreUID = storeRollingStats.StoreUID,
                        AvgOrderValue = storeRollingStats.AvgOrderValue,
                        GrowthPercentage = storeRollingStats.GrowthPercentage,
                        GRPercentage = storeRollingStats.GRPercentage,
                        OutstandingAmount = storeRollingStats.OutstandingAmount
                    };

                    storeRollingStatsList.Add(newStoreRollingStatsModel);
                }

                if (storeRollingStatsList.Any())
                {
                    await SaveStoreRollingStats(storeRollingStatsList);
                }
            }
        }


        public abstract Task<int> SaveStoreRollingStats(List<IStoreRollingStatsModel> storeRollingStatsModelList);

    }
}
