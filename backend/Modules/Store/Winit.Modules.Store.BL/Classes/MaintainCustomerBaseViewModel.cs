using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.BL.Classes
{
    public abstract class MaintainCustomerBaseViewModel : IMaintainCustomerViewModel
    {
        #region Maintain Page
        public int TotalItems { get; set; }
        public PagingRequest PagingRequest { get; set; } = new()
        {
            FilterCriterias = [],
            SortCriterias = [],
            IsCountRequired = false,
            PageNumber = 1,
            PageSize = 20
        };
        public List<IStore> Stores { get; } = [];
        public List<ISelectionItem> PriceTypeSelectionItems { get; } = [];
        public async Task OnFilterApply(IDictionary<string, string> filterCriteria)
        {
            PagingRequest.FilterCriterias?.Clear();
            PagingRequest.PageNumber = 1;
            if (filterCriteria != null)
            {
                foreach (var filter in filterCriteria)
                {
                    if (!string.IsNullOrEmpty(filter.Value))
                    {
                        FilterCriteria criteria = new FilterCriteria(filter.Key, filter.Value, FilterType.Like);

                        switch (filter.Key)
                        {
                            case nameof(IStore.SchoolWarehouse):
                                criteria.Type = FilterType.Equal;
                                break;

                            case nameof(IStore.PriceType):
                                criteria.Type = FilterType.Equal;
                                break;
                        }
                        PagingRequest.FilterCriterias?.Add(criteria);
                    }
                }
            }
            await GetStores();
        }

        public async Task OnSort(SortCriteria sortCriteria)
        {
            PagingRequest.SortCriterias.Clear();
            PagingRequest.SortCriterias.Add(sortCriteria);
        }
        public async Task OnPageChange(int pageNumber)
        {
            PagingRequest.PageNumber = pageNumber;
            await GetStores();
        }
        protected virtual Task GetStores()
        {
            return Task.CompletedTask;
        }

        #endregion



    }
}
