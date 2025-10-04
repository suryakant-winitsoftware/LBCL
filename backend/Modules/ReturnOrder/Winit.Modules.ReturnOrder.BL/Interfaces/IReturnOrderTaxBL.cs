using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ReturnOrder.BL.Interfaces
{
    public interface IReturnOrderTaxBL
    {
        Task<PagedResponse<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderTax>> SelectAllReturnOrderTaxDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Model.Interfaces.IReturnOrderTax> SelectReturnOrderTaxByUID(string UID);
        Task<int> CreateReturnOrderTax(Model.Interfaces.IReturnOrderTax returnOrderTax);
        Task<int> UpdateReturnOrderTax(Model.Interfaces.IReturnOrderTax returnOrderTax);
        Task<int> DeleteReturnOrderTax(string UID);
    }
}
