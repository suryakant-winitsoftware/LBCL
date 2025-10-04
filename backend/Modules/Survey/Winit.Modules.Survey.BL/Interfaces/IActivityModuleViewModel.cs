using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Survey.BL.Interfaces
{
    public interface IActivityModuleViewModel
    {
        public List<Winit.Modules.Survey.Model.Classes.ActivityModule> ActivityModuleList { get; set; }
        public int PageNumber { get; set; } 
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        Task PageIndexChanged(int pageNumber);
        Task OnFilterApply(Dictionary<string, string> keyValuePairs);
        public List<FilterCriteria> ActivityModuleFilterCriterias { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }
        Task GetActivityModuleData();
        Task ApplySort(SortCriteria sortCriteria);
        Task GetUsers(string OrgUID);
        Task GetStores_Customers(string orgUID);

        public List<ISelectionItem> EmpSelectionList { get; set; }

        public List<ISelectionItem> Stores_CustSelectionList { get; set; }
        public List<ISelectionItem> StateselectionItems { get; set; }
        Task GetStates();
        public List<ISelectionItem> RoleSelectionList { get; set; }
        Task GetRoles();
        public bool IsExportClicked { get; set; }

    }
}
