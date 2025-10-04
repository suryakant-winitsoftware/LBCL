using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.CollectionModule.DL.Interfaces
{
    public interface IAccPayableCMIDL
    {
        Task<PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayableCMI>> GetAccPayableCMIDetails(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string jobPositionUID);
        Task<Model.Interfaces.IAccPayableMaster> GetAccPayableMasterByUID(string uID);
        Task<List<OutstandingInvoiceView>> OutSTandingInvoicesByStoreCode(string storeCode, int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
    
}
