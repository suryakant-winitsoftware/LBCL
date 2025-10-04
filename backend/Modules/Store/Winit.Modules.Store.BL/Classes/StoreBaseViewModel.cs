using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.Store.Model. Classes;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.BL.Classes
{
    public class StoreBaseViewModel : IStoreViewModel
    {
        public int c1 { get; set; }
        public bool IsInitialized { get; set; }
        public Int64 Id { get; set; }
        public string CustomerItemViewUID { get; set; }

            
        public List<IStoreItemView> CustomerItemViews { get; set; }
        public List<IStoreItemView> FilteredCustomerItemViews { get; set; }
        public List<IStoreItemView> DisplayedCustomerItemViews { get; set; }
        public List<IStoreItemView> SelectedCustomerItemViews { get; set; }
        public List<IStoreItemView> DisplayedCustomerItemViewws_Preview { get; set; }


        public Winit.Modules.Store.Model.Interfaces.IStoreItemView SelectedStoreItemView { get; set; }
        public List<FilterCriteria> FilterCriteriaList { get; set; }
        public List<SortCriteria> SortCriteriaList { get; set; }
        public IStoreBL _StoreBL { get; set; }

        protected StoreBaseViewModel(IStoreBL storeBL)
        {
            _StoreBL = storeBL;
        }

        public Task ApplyFilter(List<FilterCriteria> filterCriterias, FilterMode filterMode)
        {
            throw new NotImplementedException();
        }

        public Task ApplySearch(string searchString)
        {
            throw new NotImplementedException();
        }

        public Task ApplySort(List<SortCriteria> sortCriterias)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();

        }

        public async Task PopulateViewModel(IStoreItemView storeViewModel,  string salesOrderUID = "")
        {
            // Sample data for testing
            CustomerItemViews =await _StoreBL.GetStoreByRouteUID("RouteUID1","",false);
            IsInitialized = true; // Set IsInitialized to true after populating data
            DisplayedCustomerItemViews = CustomerItemViews;

           
        }

        public Task SaveOrder()
        {
            throw new NotImplementedException();
        }
    }
}
