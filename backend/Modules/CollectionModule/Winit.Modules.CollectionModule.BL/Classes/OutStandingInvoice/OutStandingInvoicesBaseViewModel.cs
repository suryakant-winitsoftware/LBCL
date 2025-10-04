using Microsoft.AspNetCore.Components.Web.Virtualization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.CollectionModule.BL.Classes.OutStandingInvoice
{
    public abstract class OutStandingInvoicesBaseViewModel : IOutStandingInvoicesViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private List<string> _propertiesToSearch = new List<string>();
        private Winit.Modules.Common.BL.Interfaces.IAppUser _appUser;
        public List<IAccPayableCMI> AccPayableCMIs { get; set; }
        public IAccPayableCMI AccPayableCMIItem { get; set; }
        public IAccPayableMaster AccPayableMasterForCMIItem { get; set; }

        public PagingRequest PagingRequest { get; set; } = new()
        {
            IsCountRequired = true,
            FilterCriterias = [],
            SortCriterias = [],
            PageNumber = 1,
            PageSize = 20
        };

        public int TotalItems { get; set; }



        public OutStandingInvoicesBaseViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IAppUser appUser,
            IListHelper listHelper,
            Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appUser = appUser;
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Name");
            AccPayableCMIs = new List<IAccPayableCMI>();
            AccPayableCMIItem = new AccPayableCMI();
            AccPayableMasterForCMIItem = new AccPayableMaster()
            {
                AccPayableList = []
            };
        }

        public async virtual Task PopulateAccPayableCMI()
        {
            AccPayableCMIs = await GetAccPayableCMIFromAPI();
        }

        public async virtual Task GetAccPayableCMIByUID(string UID)
        {
            await GetAccPayableMasterFirCMIItemFromAPI(UID);
        }
        public async Task OnFilterApply(IDictionary<string, string> filterCriteria)
        {
            PagingRequest.PageNumber = 1;
            PagingRequest.FilterCriterias!.Clear();
            if (filterCriteria != null)
            {
                foreach (var filter in filterCriteria)
                {
                    if (string.IsNullOrEmpty(filter.Value))
                        continue;

                    FilterCriteria fltr = null;

                    switch (filter.Key)
                    {

                        case "Code/Name":
                            PagingRequest.FilterCriterias.Add(new FilterCriteria(nameof(IAccPayableCMI.CustomerCode),
                                filter.Value, FilterType.Like, filterMode: FilterMode.Or));
                            PagingRequest.FilterCriterias.Add(new FilterCriteria(nameof(IAccPayableCMI.CustomerName),
                                filter.Value, FilterType.Like, filterMode: FilterMode.And));
                            break;
                        default:
                            PagingRequest.FilterCriterias.Add(new FilterCriteria(filter.Key,
                                filter.Value, FilterType.Equal));
                            break;
                    }

                }
            }
            await GetPagedData();

        }
        public async Task OnSort(SortCriteria sortCriteria)
        {
            PagingRequest.SortCriterias.Clear();
            PagingRequest.SortCriterias.Add(sortCriteria);
            await GetPagedData();
        }
        public async Task OnPageChange(int pageNumber)
        {
            PagingRequest.PageNumber = pageNumber;
            await GetPagedData();
        }
        private async Task GetPagedData()
        {
            var data = await GetAccPayableCMIFromAPI();
            if (data != null)
            {
                AccPayableCMIs.Clear();
                AccPayableCMIs.AddRange(data);
            }
        }
        public abstract Task GetAccPayableMasterFirCMIItemFromAPI(string uID);
        public abstract Task<List<IAccPayableCMI>> GetAccPayableCMIFromAPI();
    }
}