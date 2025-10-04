using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ReturnOrder.DL.Interfaces
{
    public interface IReturnOrderLineDL
    {
        Task<PagedResponse<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine>> SelectAllReturnOrderLineDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);

        Task<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine> SelectReturnOrderLineByUID(string UID);
        Task<int> CreateReturnOrderLine(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine returnOrderLineDetails);

        Task<int> UpdateReturnOrderLine(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine returnOrderLineDetails);
        Task<int> DeleteReturnOrderLine(String UID);
    }
}
