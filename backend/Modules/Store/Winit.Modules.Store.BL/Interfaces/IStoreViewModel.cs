using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.BL.Interfaces
{
    public interface IStoreViewModel
    {
        public int c1 { get; set; }
        public bool IsInitialized { get; set; }
        public long Id { get; set; }
        public string CustomerItemViewUID { get; set; }


        public List<IStoreItemView> CustomerItemViews { get; set; }
        public List<IStoreItemView> FilteredCustomerItemViews { get; set; }
        public List<IStoreItemView> DisplayedCustomerItemViews { get; set; }
        public List<IStoreItemView> SelectedCustomerItemViews { get; set; }
        public List<IStoreItemView> DisplayedCustomerItemViewws_Preview { get; set; }



        public Winit.Modules.Store.Model.Interfaces.IStoreItemView SelectedStoreItemView { get; set; }
        public List<FilterCriteria> FilterCriteriaList { get; set; }
        public List<SortCriteria> SortCriteriaList { get; set; }



        Task PopulateViewModel(IStoreItemView storeViewModel, string salesOrderUID = "");

        void Dispose();

        Task ApplyFilter(List<FilterCriteria> filterCriterias, FilterMode filterMode);

        Task ApplySearch(string searchString);

        Task ApplySort(List<SortCriteria> sortCriterias);


        Task SaveOrder();

    }
}
