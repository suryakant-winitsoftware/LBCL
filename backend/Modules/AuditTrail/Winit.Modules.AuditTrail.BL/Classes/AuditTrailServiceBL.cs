using Winit.Modules.AuditTrail.BL.Interfaces;
using Winit.Modules.AuditTrail.DL.Interfaces;
using Winit.Modules.AuditTrail.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.AuditTrail.BL.Classes
{
    public class AuditTrailServiceBL : IAuditTrailServiceBL
    {
        private readonly IAuditTrailServiceDL _dataLayer;

        public AuditTrailServiceBL(IAuditTrailServiceDL dataLayer)
        {
            _dataLayer = dataLayer;
        }

        public async Task CreateAuditTrailAsync(AuditTrailEntry auditTrailEntry)
        {
            await _dataLayer.CreateAuditTrailAsync(auditTrailEntry);
        }

        public async Task<List<AuditTrailEntry>> GetAuditTrailsAsync(string linkedItemType, string linkedItemUID)
        {
            return await _dataLayer.GetAuditTrailsAsync(linkedItemType, linkedItemUID);
        }
        public async Task<AuditTrailEntry> GetAuditTrailByIdAsync(string id, bool isChangeDataRequired)
        {
            return await _dataLayer.GetAuditTrailByIdAsync(id, isChangeDataRequired);
        }
        public async Task<PagedResponse<AuditTrailEntry>> GetAuditTrailsAsyncByPaging(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _dataLayer.GetAuditTrailsAsyncByPaging(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task UpdateAuditTrailAsync(string id, AuditTrailEntry updatedAuditTrailEntry)
        {
            await _dataLayer.UpdateAuditTrailAsync(id, updatedAuditTrailEntry);
        }

        public async Task DeleteAuditTrailAsync(string id)
        {
            await _dataLayer.DeleteAuditTrailAsync(id);
        }
        public async Task<AuditTrailEntry> GetLastAuditTrailAsync(string linkedItemType, string linkedItemUID,
            bool isChangeDataRequired = true)
        {
            return await _dataLayer.GetLastAuditTrailAsync(linkedItemType, linkedItemUID, isChangeDataRequired);
        }
    }

}
