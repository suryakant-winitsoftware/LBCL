using Winit.Modules.AuditTrail.Model.Classes;
using Winit.Shared.Models.Common;

//using WINITSharedObjects.Enums;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.AuditTrail.DL.Interfaces
{
    public interface IAuditTrailServiceDL
    {
        Task CreateAuditTrailAsync(AuditTrailEntry auditTrailEntry);
        Task<List<AuditTrailEntry>> GetAuditTrailsAsync(string linkedItemType, string linkedItemUID);
        Task<AuditTrailEntry> GetAuditTrailByIdAsync(string id, bool isChangeDataRequired);
        Task<PagedResponse<AuditTrailEntry>> GetAuditTrailsAsyncByPaging(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task UpdateAuditTrailAsync(string id, AuditTrailEntry updatedAuditTrailEntry);
        Task DeleteAuditTrailAsync(string id);
        Task<AuditTrailEntry> GetLastAuditTrailAsync(string linkedItemType, string linkedItemUID,
            bool isChangeDataRequired);
    }
}
