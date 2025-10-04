using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Common.BL;
using Winit.Modules.Mobile.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Mobile.BL.Interfaces
{
    public interface IClearDataViewModel
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public IMobileAppAction MobileAppAction { get; set; }
        public List<ISelectionItem> EmpSalesRepSelectionItems { get; set; }
        public List<ISelectionItem> ActionSelectionItems { get; set; }
        public List<IMobileAppAction> ClearDataLists { get; set; }
        public List<ISelectionItem> EmpSelectionList { get; set; }

        Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias);
        Task GetSalesman(string OrgUID);
        Task ApplySort(SortCriteria sortCriteria);

        Task PopulateViewModel();
        Task PopulateViewModelForDD();
        //Task PopulateViewModelForAddEdit();
        Task SaveClearData();
        Task PageIndexChanged(int pageNumber);
        void OnCancelFromPopUp();
    }
}
