using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.JourneyPlan.BL.Interfaces;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.JourneyPlan.BL.Classes
{
    public abstract class UserJourney_AttendanceReportBaseViewModel:IUserJourney_AttendanceReportViewModel
    {
        public List<IUserJourneyGrid> userJourneyGrids { get; set; }
        public IUserJourneyView userJourneyView { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public string OrgUID { get; set; }
        public List<ISelectionItem> EmpSelectionList { get; set; }
        public List<FilterCriteria> UserJourney_AttendanceReportFilterCriterias { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }

        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        public UserJourney_AttendanceReportBaseViewModel(IServiceProvider serviceProvider,
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
            userJourneyGrids= new List<IUserJourneyGrid>();
            userJourneyView=new Winit.Modules.JourneyPlan.Model.Classes.UserJourneyView();
            EmpSelectionList = new List<ISelectionItem>();
            UserJourney_AttendanceReportFilterCriterias = new List<FilterCriteria>();
            SortCriterias = new List<SortCriteria>();
        }
        public async Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias)
        {
            UserJourney_AttendanceReportFilterCriterias.Clear();
            UserJourney_AttendanceReportFilterCriterias.AddRange(filterCriterias);
            await PopulateViewModel();
        }
        public async Task ApplySort(SortCriteria sortCriteria)
        {
            SortCriterias.Clear();
            SortCriterias.Add(sortCriteria);
            await PopulateViewModel();
        }
        public async Task GetSalesman(string OrgUID)
        {
            EmpSelectionList.Clear();
            EmpSelectionList.AddRange(await GetSalesmanData(OrgUID));
        }
        public async virtual Task PopulateViewModel()
        {
            userJourneyGrids = await GetUserJourneyGridData();
        }
        public async Task PopulateUserJourneyReportforView(string UID)
        {
            userJourneyView = await GetUserJourneyReportforView(UID);
        }
        public async Task PageIndexChanged(int pageNumber)
        {
            PageNumber = pageNumber;
            await PopulateViewModel();
        }
        public abstract Task<List<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourneyGrid>> GetUserJourneyGridData();
        public abstract Task<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourneyView> GetUserJourneyReportforView(string UID);
        public abstract Task<List<ISelectionItem>> GetSalesmanData(string OrgUID);

    }
}
