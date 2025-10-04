using Winit.Modules.AuditTrail.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.AuditTrail.BL.Interfaces
{
    public interface IAuditTrailView
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalAuditTrailItems { get; set; }
        public List<IAuditTrailEntry> AuditTrailEntry { get; set; }
        public IAuditTrailEntry CurrentAuditTrailEntry { get; set; }
        public IAuditTrailEntry OriginalAuditTrailEntry { get; set; }
        public List<ChangeLog> ChangeData { get; set; }
        public List<ChangeLog> FilteredChangeData { get; set; }
        Task GetAuditTrailAsync();
        Task GetAuditTrailByIdAndPopulateViewModel(string id, bool isChangeDataRequired);
        Task LoadOldRequestData();
        Task OnFilterApply(Dictionary<string, string> filterCriteria);
        Task ApplySort(SortCriteria sortCriteria);
        Task PageIndexChanged(int pageNumber);
        Task<List<ISelectionItem>> GetModuleDropdownValuesAsync();

    }
}
