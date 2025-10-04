using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKUClass.BL.UIInterfaces;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKUClass.BL.UIClasses
{
    public abstract class SKUClassGroupBaseViewModel : ISKUClassGroupViewModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalSKUClassGroupItemsCount { get; set; }
        public ISKUClassGroup? SKUClassGroup { get; set; }
        public List<ISKUClassGroup> SKUClassGroupsList { get; set; }
        public List<FilterCriteria> SKUClassGroupFilterCriterias { get; set; }
        public List<SortCriteria> SKUClassGroupSortCriterials { get; set; }

        // Injection
        private readonly IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private readonly IAppUser _appUser;
        private readonly IAppSetting _appSetting;


        protected SKUClassGroupBaseViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper,
            IAppUser appUser,
            IAppSetting appSetting
        )
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _appUser = appUser;
            _appSetting = appSetting;
            // Initialize common properties or perform other common setup
            SKUClassGroupSortCriterials = new List<SortCriteria>();
            SKUClassGroupFilterCriterias = new List<FilterCriteria>();
            SKUClassGroupsList = new List<ISKUClassGroup>();
        }

        public async Task PopulateViewModel()
        {
            List<ISKUClassGroup> data = await GetSKUClassGroups();
            if (data != null)
            {
                SKUClassGroupsList.Clear();
                SKUClassGroupsList.AddRange(data);
            }
        }

        protected abstract Task<List<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup>> GetSKUClassGroups();
        protected abstract Task<bool> DeleteSKUClassGroupMaster(string skuClassGroupUID);

        public async Task ApplyFilter(IDictionary<string, string> keyValuePairs)
        {
            PageNumber = 1;
            SKUClassGroupFilterCriterias.Clear();
            foreach (KeyValuePair<string, string> keyvalue in keyValuePairs)
            {
                if (!string.IsNullOrEmpty(keyvalue.Value))
                {
                    if (keyvalue.Key.Equals(nameof(ISKUClassGroup.FromDate)))
                        SKUClassGroupFilterCriterias.Add(new FilterCriteria(keyvalue.Key,
                            CommonFunctions.GetDate(keyvalue.Value), FilterType.GreaterThanOrEqual));
                    else if (keyvalue.Key.Equals(nameof(ISKUClassGroup.ToDate)))
                        SKUClassGroupFilterCriterias.Add(new FilterCriteria(keyvalue.Key,
                            CommonFunctions.GetDate(keyvalue.Value), FilterType.LessThanOrEqual));
                    else
                        SKUClassGroupFilterCriterias.Add(new FilterCriteria(keyvalue.Key, keyvalue.Value,
                            FilterType.Like));
                }
            }

            await PopulateViewModel();
        }

        public async Task ResetFilter()
        {
            SKUClassGroupFilterCriterias.Clear();
            await PopulateViewModel();
        }

        /// <summary>
        /// This will sort data from FilteredTaxItemViews and store in FilteredTaxItemViews & DisplayedTaxItemViews
        /// </summary>
        /// <param name="sortCriterias"></param>
        /// <returns></returns>
        public async Task ApplySort(List<Shared.Models.Enums.SortCriteria> sortCriterias)
        {
            await Task.CompletedTask;
        }

        public async Task PageIndexChanged(int pageNumber)
        {
            PageNumber = pageNumber;
            await PopulateViewModel();
        }

        public async Task<bool> OnSKUClassGroupDeleteClick(ISKUClassGroup skuClassGroup)
        {
            if (await DeleteSKUClassGroupMaster(skuClassGroup.UID))
            {
                _ = SKUClassGroupsList.Remove(skuClassGroup);
                return true;
            }

            return false;
        }
    }
}