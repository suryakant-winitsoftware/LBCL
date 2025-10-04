using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.CollectionModule.BL.Interfaces
{
    public interface IViewPaymentsViewModel
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } 
        public int PageSize { get; set; } 
        public List<IAccCollection> Payments { get; set; }
        public List<IAccCollection> CollectionTabDetails { get; set; } 
        public List<FilterCriteria> FilterCriterias { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }
        public List<Winit.Modules.Store.Model.Classes.Store> CustomerCode { get; set; }
        public List<AccCollectionAllotment> _viewDetailsList { get; set; }
        public Dictionary<string, string> storeData { get; set; }
        Task OnFilterApply(Dictionary<string, string> keyValuePairs, string pageName);
        Task OnSortApply(SortCriteria sortCriteria);
        Task PopulateViewModel();
        Task<List<IAccCollection>> GetReceiptDetails_Data();
        Task ViewReceiptDetails(string UID);
        Task GetCustomerCodeName();
        Task CollectionTabs();
    }
}
