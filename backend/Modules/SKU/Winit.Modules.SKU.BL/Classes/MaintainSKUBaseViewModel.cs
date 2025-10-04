using Nest;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Microsoft.Extensions.Logging;

namespace Winit.Modules.SKU.BL.Classes
{
    /// <summary>
    /// Base view model for maintaining SKU functionality
    /// </summary>
    public abstract class MaintainSKUBaseViewModel : IMaintainSKUViewModel
    {
        private readonly ILogger<MaintainSKUBaseViewModel> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private readonly IAppUser _appUser;
        private readonly IAppSetting _appSetting;
        private readonly IDataManager _dataManager;
        protected readonly IAppConfig _appConfigs;
        protected readonly Base.BL.ApiService _apiService;

        /// <summary>
        /// Gets or sets the current page number
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Gets or sets the number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total count of SKU items
        /// </summary>
        public int TotalSKUItemsCount { get; set; }

        /// <summary>
        /// Gets or sets the list of SKU items for grid display
        /// </summary>
        public List<ISKUListView> MaintainSKUGridList { get; set; }

        /// <summary>
        /// Gets or sets the list of filter criteria
        /// </summary>
        public List<FilterCriteria> SKUFilterCriterias { get; set; }

        /// <summary>
        /// Gets or sets the list of attribute type selection items
        /// </summary>
        public List<ISelectionItem> AttributeTypeSelectionItems { get; set; }

        /// <summary>
        /// Gets or sets the list of attribute name selection items
        /// </summary>
        public List<ISelectionItem> AttributeNameSelectionItems { get; set; }

        /// <summary>
        /// Gets or sets the list of status selection items
        /// </summary>
        public List<ISelectionItem> StatusSelectionItems { get; set; }

        /// <summary>
        /// Gets or sets the SKU attribute level information
        /// </summary>
        public ISKUAttributeLevel SkuAttributeLevel { get; set; }

        /// <summary>
        /// Gets or sets the list of sort criteria
        /// </summary>
        public List<SortCriteria> SortCriterias { get; set; }

        /// <summary>
        /// Gets or sets the list of product division selection items
        /// </summary>
        public List<ISelectionItem> ProductDivisionSelectionItems { get; set; }

        /// <summary>
        /// Initializes a new instance of the MaintainSKUBaseViewModel class
        /// </summary>
        protected MaintainSKUBaseViewModel(
            IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper,
            IAppUser appUser,
            IAppSetting appSetting,
            IDataManager dataManager,
            IAppConfig appConfigs,
            Base.BL.ApiService apiService,
            ILogger<MaintainSKUBaseViewModel> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _filter = filter ?? throw new ArgumentNullException(nameof(filter));
            _sorter = sorter ?? throw new ArgumentNullException(nameof(sorter));
            _listHelper = listHelper ?? throw new ArgumentNullException(nameof(listHelper));
            _appUser = appUser ?? throw new ArgumentNullException(nameof(appUser));
            _appSetting = appSetting ?? throw new ArgumentNullException(nameof(appSetting));
            _dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
            _appConfigs = appConfigs ?? throw new ArgumentNullException(nameof(appConfigs));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            MaintainSKUGridList = new List<ISKUListView>();
            SKUFilterCriterias = new List<FilterCriteria>();
            AttributeNameSelectionItems = new List<ISelectionItem>();
            StatusSelectionItems = new List<ISelectionItem>();
            SortCriterias = new List<SortCriteria>();
            ProductDivisionSelectionItems = new List<ISelectionItem>();
        }

        /// <summary>
        /// Initializes the status selection items
        /// </summary>
        public async Task GetStatus()
        {
            try
            {
                StatusSelectionItems.Add(new SelectionItem { UID = "true", Code = "true", Label = "Yes" });
                StatusSelectionItems.Add(new SelectionItem { UID = "false", Code = "false", Label = "No" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting status");
                throw;
            }
        }

        /// <summary>
        /// Applies filter criteria to the SKU list
        /// </summary>
        public async Task OnFilterApply(List<UIModels.Common.Filter.FilterModel> columnsForFilter, Dictionary<string, string> filterCriteria)
        {
            if (columnsForFilter == null) throw new ArgumentNullException(nameof(columnsForFilter));
            if (filterCriteria == null) throw new ArgumentNullException(nameof(filterCriteria));

            try
            {
                var filterCriterias = new List<FilterCriteria>();
                foreach (var keyValue in filterCriteria)
                {
                    if (string.IsNullOrEmpty(keyValue.Value)) continue;

                    switch (keyValue.Key)
                    {
                        case "AttributeType":
                            var selectionItem = SkuAttributeLevel.SKUGroupTypes.Find(e => e.Code == keyValue.Value);
                            if (selectionItem != null)
                            {
                                filterCriterias.Add(new FilterCriteria(keyValue.Key, new List<string> { selectionItem.Code }, FilterType.Equal));
                            }
                            break;

                        case "AttributeValue":
                            var selectedUids = keyValue.Value.Split(",");
                            var selectedLabels = AttributeNameSelectionItems
                                .Where(e => selectedUids.Contains(e.UID))
                                .Select(_ => _.UID);
                            filterCriterias.Add(new FilterCriteria(keyValue.Key, selectedLabels, FilterType.In));
                            break;

                        case "IsActive":
                            filterCriterias.Add(new FilterCriteria(keyValue.Key, keyValue.Value == "True" ? 0 : 1, FilterType.Equal));
                            break;

                        case "DivisionUID":
                            filterCriterias.Add(new FilterCriteria("DivisionUIDs", keyValue.Value.Trim().Split(","), FilterType.In));
                            break;

                        default:
                            filterCriterias.Add(new FilterCriteria(keyValue.Key, keyValue.Value, FilterType.Like));
                            break;
                    }
                }
                await ApplyFilter(filterCriterias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while applying filters");
                throw;
            }
        }

        public async Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias)
        {
            SKUFilterCriterias.Clear();
            SKUFilterCriterias.AddRange(filterCriterias);
            await PopulateViewModel();
        }
        public async Task ApplySort(SortCriteria sortCriteria)
        {
            SortCriterias.Clear();
            SortCriterias.Add(sortCriteria);
            await PopulateViewModel();
        }
        public async Task ResetFilter()
        {
            SKUFilterCriterias.Clear();
            await PopulateViewModel();
        }
        public async Task<bool> CheckUserExistsAsync(string code)
        {
            if (MaintainSKUGridList == null || MaintainSKUGridList.Count <= 0)
            {
                await GetMaintainSKUGridData();

            }
            return await Task.FromResult(
                MaintainSKUGridList.Any(u => u.SKUCode == code)
            );
        }
        public virtual async Task PopulateViewModel()
        {
           await GetMaintainSKUGridData();
        }
        public async Task PageIndexChanged(int pageNumber)
        {
            PageNumber = pageNumber;
            await PopulateViewModel();
        }
        #region Business Logics  
        public async Task<string> DeleteSKUItem(string UID)
        {
            return await DeleteSKU(UID);
        }
        public async Task<ISKUAttributeLevel> GetAttributeType()
        {
            return await GetSKUAttributeType();
        }
        public async Task OnAttributeTypeSelect(string code)
        {
            AttributeNameSelectionItems.Clear();
            AttributeNameSelectionItems.AddRange(SkuAttributeLevel.SKUGroups[code]);
        }
        public async Task OnDivisionSelectionTypeSelect()
        {
            ProductDivisionSelectionItems.Clear();
            ProductDivisionSelectionItems.AddRange(_appUser.ProductDivisionSelectionItems?.Select(e => (e as SelectionItem).DeepCopy()));
        }
        #endregion
        #region Database or Services Methods
        public abstract Task GetMaintainSKUGridData();
        public abstract Task<ISKUAttributeLevel> GetSKUAttributeType();
        public abstract Task<string> DeleteSKU(string uid);
        #endregion
    }
}
