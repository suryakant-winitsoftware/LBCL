using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Survey.Model.Classes;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Modules.User.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Survey.BL.Interfaces
{
    public interface ISurveyResponseViewModel : Winit.Modules.Base.BL.Interfaces.ITableGridViewModel
    {
        string PageType { get; set; }
        public string Role { get; set; }
        public List<IViewSurveyResponse> SurveyResponsesList { get; set; }
        public List<IViewSurveyResponseExport> ExporttoExcelSurveyResponsesList { get; set; }

        public ISurveyResponseViewDTO ViewSurveyResponsesDTOList { get; set; }
        Task GetSurveyResponseDetailsForExport();
        Task PopulateViewModel();
        Task ViewSurveyResponsePopulateViewModel(string SurveyUID);
        public int _TotalCategoryCount { get; set; }
        public int _ExcecutedCount { get; set; }
        public int _pendingCount { get; set; }
        public List<IViewSurveyResponse> _filteredSurveyResponseList { get; set; }
        Task RefreshUserVisitStatusCountsAsync();
        public List<IViewSurveyResponse> TabCountforTotalCategoryList { get; set; }
        Task GetTabDataAfterFilterApply();
        public bool IspageInitalLoad { get; set; }
        public bool IsFilerappliedForTabData { get; set; }

        public List<SortCriteria> SortCriterias { get; set; }
        Task ApplySort(SortCriteria sortCriteria);
        public string OrgUID { get; set; }

        Task GetUsers(string OrgUID);
        Task GetStores_Customers(string orgUID);  

        public List<ISelectionItem> EmpSelectionList { get; set; }
        public List<ISelectionItem> RoleSelectionList { get; set; }

        public List<ISelectionItem> Stores_CustSelectionList { get; set; }
        public List<ISelectionItem> StateselectionItems { get; set; }
        Task GetStates();
        Task GetRoles();
        Task<List<ISelectionItem>> GetQuestionLabelsBySurveyUid(string selectedSurveyUid);
        public List<ISelectionItem> QuestionsList { get; set; }

    }
}
