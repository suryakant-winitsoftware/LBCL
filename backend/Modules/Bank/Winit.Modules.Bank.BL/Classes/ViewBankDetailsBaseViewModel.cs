using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Bank.BL.Interfaces;
using Winit.Modules.Bank.Model.Interfaces;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Bank.BL.Classes;

public abstract class ViewBankDetailsBaseViewModel : IViewBankDetailsViewModel
{
    public List<ISelectionItem> CountrySelectionItems { get; set; }
    public List<IBank> BankDetailsList { get; set; }
    public IBank ViewBankDetails { get; set; }
    private IServiceProvider _serviceProvider;
    public List<FilterCriteria> BankDetailsFilterCriteria { get; set; }

    private readonly IFilterHelper _filter;
    private readonly ISortHelper _sorter;
    private readonly IListHelper _listHelper;
    private Winit.Shared.Models.Common.IAppConfig _appConfigs;
    private Winit.Modules.Base.BL.ApiService _apiService;
    private List<string> _propertiesToSearch = new List<string>();
    public ViewBankDetailsBaseViewModel(IServiceProvider serviceProvider,
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
        BankDetailsList = new List<IBank>();
        BankDetailsFilterCriteria = new List<FilterCriteria>();
        CountrySelectionItems = new List<ISelectionItem>();
        // Property set for Search
        _propertiesToSearch.Add("Code");
        _propertiesToSearch.Add("Name");
    }

    public async virtual Task PopulateViewModel()
    {
        await GetBankDetailsdata();
        List<string> locationTypes = new List<string>();
        locationTypes.Add("Country");
        var locationsData = await GetCountryDetailsSelectionItems(locationTypes);
        if (locationsData != null && locationsData.Any())
        {
            CountrySelectionItems.Clear();
            CountrySelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems<ILocation>(locationsData, new List<string>
            {
                "UID",
                "Code",
                "Name"
            }));
        }
    }
    public async Task GetBankDetailsdata()
    {
        BankDetailsList = await GetBankDetailsData();

    }
    public async virtual Task PopulateBankViewDetailsByUID(string UID)
    {
        ViewBankDetails = await GetBankViewDatailsData(UID);
        var selectedCountry = CountrySelectionItems.Find(e => e.UID == ViewBankDetails.CountryUID);
        if (selectedCountry != null)
        {
            selectedCountry.IsSelected = true;
        }
    }
    public abstract Task<List<Winit.Modules.Bank.Model.Interfaces.IBank>> GetBankDetailsData();
    public abstract Task<List<ILocation>?> GetCountryDetailsSelectionItems(List<string> locationTypes);
    public abstract Task<Winit.Modules.Bank.Model.Interfaces.IBank> GetBankViewDatailsData(string UID);
    public abstract Task<bool> CreateUpdateBankDetailsData(IBank bank, bool Operation);
    public abstract Task<string> DeleteVehicle(object uID);
    public async Task ApplyFilter(List<FilterCriteria> filterCriterias)
    {
        BankDetailsFilterCriteria.Clear();
        BankDetailsFilterCriteria.AddRange(filterCriterias);
        await GetBankDetailsdata();
    }

}
