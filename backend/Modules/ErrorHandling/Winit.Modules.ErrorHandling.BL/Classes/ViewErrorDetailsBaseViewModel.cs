using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.ErrorHandling.BL.Interfaces;
using Winit.Modules.ErrorHandling.Model.Interfaces;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.ErrorHandling.BL.Classes
{
    public abstract class ViewErrorDetailsBaseViewModel : IViewErrorDetailsViewModel
    {
        public List<IErrorDetail> ErrorDetailsList { get; set; }
        public List<FilterCriteria> ErrorDetailsFilterCriteria { get; set; }
        public List<SortCriteria> ErrorDetailsSortCriterias { get; set; }
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private IServiceProvider _serviceProvider;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private List<string> _propertiesToSearch = new List<string>();
        public ViewErrorDetailsBaseViewModel(IServiceProvider serviceProvider,
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
            ErrorDetailsList = new List<IErrorDetail>();
            ErrorDetailsFilterCriteria = new List<FilterCriteria>();
            // Property set for Search
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Name");
        }
        public async virtual Task PopulateViewModel()
        {
            ErrorDetailsList = await GetErrorDetailsData();
        
    }
        public async Task ApplyFilter(List<FilterCriteria> filterCriterias)
        {
            ErrorDetailsFilterCriteria.Clear();
            ErrorDetailsFilterCriteria.AddRange(filterCriterias);
            await PopulateViewModel();
        }
        public async Task ApplySort(SortCriteria sortCriteria)
        {
            ErrorDetailsSortCriterias.Clear();
            ErrorDetailsSortCriterias.Add(sortCriteria);
            await PopulateViewModel();
        }
        public abstract Task<List<Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDetail>> GetErrorDetailsData();

    }
}
