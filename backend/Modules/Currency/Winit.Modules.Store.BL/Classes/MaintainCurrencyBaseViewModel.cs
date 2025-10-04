using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Currency.BL.Interfaces;
using Winit.Modules.Currency.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.Currency.BL.Classes
{
    public abstract class MaintainCurrencyBaseViewModel :IMaintainCurrencyViewModel
    {
        public List<ISelectionItem> DigitsSelectionItems { get; set; }
        public List<ICurrency> CurrencyDetailsList { get; set; }
        public ICurrency ViewCurrencyDetails { get; set; }
        private IServiceProvider _serviceProvider;
        public List<FilterCriteria> CurrencyDetailsFilterCriteria { get; set; }

        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private List<string> _propertiesToSearch = new List<string>();
        public MaintainCurrencyBaseViewModel(IServiceProvider serviceProvider,
               IFilterHelper filter,
               ISortHelper sorter,
               IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService
             )
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            CurrencyDetailsList = new List<ICurrency>();
            CurrencyDetailsFilterCriteria = new List<FilterCriteria>();
           DigitsSelectionItems = new List<ISelectionItem>();
            GetDigits();
            // Property set for Search
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Name");
        }
        private void GetDigits()
        {
            DigitsSelectionItems.Add(new SelectionItem { UID = "0", Code = "one", Label = "0" });
            DigitsSelectionItems.Add(new SelectionItem { UID = "1", Code = "two", Label = "1" });
            DigitsSelectionItems.Add(new SelectionItem { UID = "2", Code = "three", Label = "2" });
            DigitsSelectionItems.Add(new SelectionItem { UID = "3", Code = "four", Label = "3" });
            DigitsSelectionItems.Add(new SelectionItem { UID = "4", Code = "five", Label = "4" });
            DigitsSelectionItems.Add(new SelectionItem { UID = "5", Code = "six", Label = "5" });
        }
        public async virtual Task PopulateViewModel()
        {
            
            CurrencyDetailsList = await GetCurrencyDetailsData();
        }
        public async virtual Task PopulateCurrencyViewDetailsByUID(string UID)
        {
            ViewCurrencyDetails = await GetCurrencyViewDetailsData(UID);
            var selectedDigits = DigitsSelectionItems.Find(e => e.UID == ViewCurrencyDetails.Digits.ToString());
            if (selectedDigits != null)
            {
                selectedDigits.IsSelected = true;
            }
        }
        public abstract Task<List<Winit.Modules.Currency.Model.Interfaces.ICurrency>> GetCurrencyDetailsData();
      //  public abstract Task<List<ILocation>?> GetCountryDetailsSelectionItems(List<string> locationTypes);
        public abstract Task<Winit.Modules.Currency.Model.Interfaces.ICurrency> GetCurrencyViewDetailsData(string UID);
        public abstract Task<bool> CreateUpdateCurrencyDetailsData(ICurrency bank, bool Operation);
        public abstract Task<string> DeleteCurrency(object uID);
        public async Task ApplyFilter(List<FilterCriteria> filterCriterias)
        {
            CurrencyDetailsFilterCriteria.Clear();
            CurrencyDetailsFilterCriteria.AddRange(filterCriterias);
            await PopulateViewModel();
        }
    }
}
