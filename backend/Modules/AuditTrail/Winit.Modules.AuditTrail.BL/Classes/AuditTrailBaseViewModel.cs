using Winit.Modules.AuditTrail.BL.Interfaces;
using Winit.Modules.AuditTrail.Model.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.AuditTrail.BL.Classes
{
    public abstract class AuditTrailBaseViewModel : IAuditTrailView
    {
        protected PagingRequest PagingRequest = new PagingRequest()
        {
            IsCountRequired = true,
            FilterCriterias = []
        };
        public List<IAuditTrailEntry> AuditTrailEntry { get; set; }
        public IAuditTrailEntry CurrentAuditTrailEntry { get; set; }
        public IAuditTrailEntry OriginalAuditTrailEntry { get; set; }
        public List<ChangeLog> ChangeData { get; set; }
        public List<ChangeLog> FilteredChangeData { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalAuditTrailItems { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }

        public IServiceProvider _serviceProvider;
        protected readonly IAppUser _appUser;
        public Winit.Shared.Models.Common.IAppConfig _appConfigs;
        public Winit.Modules.Base.BL.ApiService _apiService;
        public AuditTrailBaseViewModel(IServiceProvider serviceProvider,
            IAppUser appUser,
            Shared.Models.Common.IAppConfig appConfigs,
            Base.BL.ApiService apiService)
        {
            _serviceProvider = serviceProvider;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appUser = appUser;
            SortCriterias = new List<SortCriteria>();
        }
        public abstract Task PopulateViewModel();
        public abstract Task GetAuditTrailAsync();
        public abstract Task GetAuditTrailByIdAndPopulateViewModel(string id, bool isChangeDataRequired);
        public abstract Task LoadOldRequestData();
        public abstract Task OnFilterApply(Dictionary<string, string> filterCriteria);
        public abstract Task ApplySort(SortCriteria sortCriteria);
        public abstract Task PageIndexChanged(int pageNumber);
        public abstract Task<List<ISelectionItem>> GetModuleDropdownValuesAsync();
    }
}
