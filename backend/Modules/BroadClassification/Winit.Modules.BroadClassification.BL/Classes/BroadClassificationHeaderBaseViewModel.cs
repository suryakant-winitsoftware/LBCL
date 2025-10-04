using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.BroadClassification.BL.Interfaces;
using Winit.Modules.BroadClassification.Model.Interfaces;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.BroadClassification.BL.Classes
{
    public abstract class BroadClassificationHeaderBaseViewModel : IBroadClassificationHeaderViewModel
    {
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        public Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private IServiceProvider _serviceProvider;
        //public int PageNumber { get; set; } = 1;
        //public int PageSize { get; set; } = 50;
        public int TotalItemsCount { get; set; }

        public PagingRequest PagingRequest { get; set; } = new PagingRequest()
        {
            FilterCriterias = [],
            IsCountRequired = true,
            SortCriterias = [],
            PageNumber = 1,
            PageSize = 50,
        };
        public List<IBroadClassificationHeader> broadClassificationHeaderslist { get; set; }
        public IBroadClassificationHeader viewBroadClassificationHeaderLineData { get; set; }
        public List<IListItem> ClassificationTypes { get; set; }

        public BroadClassificationHeaderBaseViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            broadClassificationHeaderslist = new List<IBroadClassificationHeader>();
            ClassificationTypes = new List<IListItem>();
            // Ensure initialization here
        }



        public async virtual Task PopulateViewModel()
        {
            await GetBroadClassificationHeaderdata();
            String ClassificationType = "CustomerClassifciation";
            ClassificationTypes = await GetClassificationsList(ClassificationType);
        }

        public async Task PageIndexChanged(int pageNumber)
        {
            PagingRequest.PageNumber = pageNumber;
            await PopulateViewModel();
        }

        public async virtual Task PopulateBroadClassificationHeaderDetailsByUID(string UID)
        {
            viewBroadClassificationHeaderLineData = await GetBroadClassificationHeaderDetailsByUID(UID);
        }

        private async Task GetBroadClassificationHeaderdata()
        {
            this.broadClassificationHeaderslist.Clear();
            var broadClassificationHeaderslist = await GetBroadClassificationHeaderData();
            if (broadClassificationHeaderslist != null)
            {
                this.broadClassificationHeaderslist.AddRange(broadClassificationHeaderslist);
            }
        }
        public async Task ApplyFilter(List<FilterCriteria> filterCriterias)
        {
            PagingRequest.FilterCriterias!.Clear();
            PagingRequest.FilterCriterias.AddRange(filterCriterias);
            await GetBroadClassificationHeaderdata();
        }
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; } = 1;

        public async Task OnSort(SortCriteria sortCriteria)
        {
            PagingRequest.SortCriterias!.Clear();
            PagingRequest.SortCriterias.Add(sortCriteria);
            await GetBroadClassificationHeaderdata();
        }
        public async Task OnPageChange(int pageNumber)
        {
            PagingRequest.PageNumber = CurrentPage = pageNumber;
            await GetBroadClassificationHeaderdata();
        }
        public abstract Task<List<IBroadClassificationHeader>> GetBroadClassificationHeaderData();
        public abstract Task<IBroadClassificationHeader> GetBroadClassificationHeaderDetailsByUID(string UID);
        public abstract Task<bool> CreateUpdateBroadClassificationHeaderData(IBroadClassificationHeader broadClassificationHeader, bool Operation);
        public abstract Task<string> DeleteBroadClassificationHeaderData(object uID);
        public abstract Task<List<IListItem>?> GetClassificationsList(string classificationTypes);

    }
}
