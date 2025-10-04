using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StoreDocument.BL.Interfaces
{
    public interface IStoreDocumentBL
    {
        Task<PagedResponse<Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument>> SelectAllStoreDocumentDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument> GetStoreDocumentDetailsByUID(string UID);
        Task<int> CreateStoreDocumentDetails(Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument createStoreDocument);
        Task<int> UpdateStoreDocumentDetails(Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument updateStoreDocument);
        Task<int> DeleteStoreDocumentDetails(String UID);
    }
}
