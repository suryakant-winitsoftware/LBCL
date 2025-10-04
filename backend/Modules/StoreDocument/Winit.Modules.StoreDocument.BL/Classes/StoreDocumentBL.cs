using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StoreDocument.BL.Classes
{
    public class StoreDocumentBL : StoreDocumentBaseBL, Interfaces.IStoreDocumentBL
    {
        protected readonly DL.Interfaces.IStoreDocumentDL _StoreDocumentBL = null;
        public StoreDocumentBL(DL.Interfaces.IStoreDocumentDL StoreDocumentBL)
        {
            _StoreDocumentBL = StoreDocumentBL;
        }
        public async Task<PagedResponse<Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument>>SelectAllStoreDocumentDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _StoreDocumentBL.SelectAllStoreDocumentDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Model.Interfaces.IStoreDocument> GetStoreDocumentDetailsByUID(string UID)
        {
            return await _StoreDocumentBL.GetStoreDocumentDetailsByUID(UID);
        }
        public async Task<int> CreateStoreDocumentDetails(Model.Interfaces.IStoreDocument createStoreDocument)
        {
            return await _StoreDocumentBL.CreateStoreDocumentDetails(createStoreDocument);
        }
        public async Task<int> UpdateStoreDocumentDetails(Model.Interfaces.IStoreDocument updateStoreDocument)
        {
            return await _StoreDocumentBL.UpdateStoreDocumentDetails(updateStoreDocument);
        }
        public async Task<int> DeleteStoreDocumentDetails(string UID)
        {
            return await _StoreDocumentBL.DeleteStoreDocumentDetails(UID);
        }
    }
}
