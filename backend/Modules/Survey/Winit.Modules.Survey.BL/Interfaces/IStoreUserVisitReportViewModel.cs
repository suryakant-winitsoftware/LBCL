using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Survey.BL.Interfaces
{
    public interface IStoreUserVisitReportViewModel :Winit.Modules.Base.BL.Interfaces.ITableGridViewModel
    {
        public List<IStoreUserVisitDetails> ?StoreUserVisitDetails { get; set; }
        public List<FilterCriteria> StoreUserVisitReportFilterCriterias { get; set; }
        public List<IStoreUserVisitDetails>? StoreUserVisitDetailsForExport { get; set; }

        Task PopulateViewModel();
        public int _plannedCount { get; set; }
        public int _visitedCount { get; set; }
        public int _unPlannedCount { get; set; }
        Task RefreshUserVisitStatusCountsAsync();
        Task ExporttoExcel();
        public List<ISelectionItem> StateselectionItems { get; set; }
        Task GetStates();
        Task GetStores_Customers(string orgUID);
        public string OrgUID { get; set; }
        public List<ISelectionItem> EmpSelectionList { get; set; }
        public List<ISelectionItem> StoreSelectionList { get; set; }

        Task GetSalesman(string OrgUID);
        public List<ISelectionItem> RoleSelectionList { get; set; }
        Task GetRoles();

    }
}
