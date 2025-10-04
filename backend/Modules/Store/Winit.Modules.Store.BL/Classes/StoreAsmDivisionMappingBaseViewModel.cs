using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Services;

namespace Winit.Modules.Store.BL.Classes
{
    public abstract class StoreAsmDivisionMappingBaseViewModel : IStoreAsmDivisionMappingViewModel
    {
        protected readonly Winit.Shared.Models.Common.IAppConfig _appConfig;
        protected readonly ApiService _apiService;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IAppUser _appUser;
        protected readonly ILoadingService _loadingService;
        protected readonly IAlertService _alertService;
        protected readonly IAppSetting _appSetting;
        protected readonly CommonFunctions _commonFunctions;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItemsCount { get; set; }
        public List<FilterCriteria> FilterCriterias { get; set; } = new List<FilterCriteria>();
        public List<SortCriteria> SortCriterias { get; set; }
        public SortCriteria DefaultSortCriteria = new("CreatedTime", SortDirection.Desc);
        public List<IAsmDivisionMapping> StoreAsmMappingGridViewRecords { get; set; }
        public List<StoreAsmMapping> StoreAsmMappingErrorRecords { get; set; }
        public StoreAsmDivisionMappingBaseViewModel(IServiceProvider serviceProvider, IFilterHelper filter, ISortHelper sorter, IListHelper listHelper, IAppUser appUser, IAppSetting appSetting,
        IDataManager dataManager, Shared.Models.Common.IAppConfig appConfigs, ApiService apiService, CommonFunctions commonFunctions)
        {
            _apiService = apiService;
            _appConfig = appConfigs;
            _serviceProvider = serviceProvider;
            _appUser = appUser;
            _appSetting = appSetting;
            _commonFunctions = commonFunctions;
            StoreAsmMappingGridViewRecords = new List<IAsmDivisionMapping>();
        }
        public async Task PopulateViewModel()
        {
            try
            {
                StoreAsmMappingGridViewRecords = await PopulateUI();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task OnFileUploadInsertIntoDB(List<StoreAsmMapping> storeAsmMappingRecords)
        {
            try
            {
                await InsertIntoDB(storeAsmMappingRecords);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public async Task PageIndexChanged(int pageNumber)
        {
            PageNumber = pageNumber;
            await PopulateViewModel();
        }
        public async Task OnSorting(SortCriteria sortCriteria)
        {
            try
            {
                SortCriterias.Clear();
                SortCriterias.Add(sortCriteria);
                await PopulateViewModel();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public async Task OnFilterApply(Dictionary<string, string> keyValuePairs)
        {
            try
            {
                FilterCriterias.Clear();
                foreach (var keyValue in keyValuePairs)
                {
                    if (!string.IsNullOrEmpty(keyValue.Value))
                    {
                        if (keyValue.Value.Contains(","))
                        {
                            string[] values = keyValue.Value.Split(',');
                            FilterCriterias.Add(new FilterCriteria(keyValue.Key, values, FilterType.In));
                        }
                        else
                        {
                            FilterCriterias.Add(new FilterCriteria(keyValue.Key, keyValue.Value, FilterType.Like));
                        }
                    }
                }
                await PopulateViewModel();
            }
            catch (Exception ex)
            {

            }
        }
        public abstract Task<List<IAsmDivisionMapping>> PopulateUI();
        public abstract Task InsertIntoDB(List<StoreAsmMapping> storeAsmMappingRecords);
    }
}
