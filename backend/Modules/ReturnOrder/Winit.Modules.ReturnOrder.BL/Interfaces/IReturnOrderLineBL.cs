using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ReturnOrder.BL.Interfaces
{
    public interface IReturnOrderLineBL
    {
        Task<PagedResponse<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine>> SelectAllReturnOrderLineDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Model.Interfaces.IReturnOrderLine> SelectReturnOrderLineByUID(string UID);
        Task<int> CreateReturnOrderLine(Model.Interfaces.IReturnOrderLine returnOrderLine);
        Task<int> UpdateReturnOrderLine(Model.Interfaces.IReturnOrderLine returnOrderLine);
        Task<int> DeleteReturnOrderLine(string UID);
    }
}
