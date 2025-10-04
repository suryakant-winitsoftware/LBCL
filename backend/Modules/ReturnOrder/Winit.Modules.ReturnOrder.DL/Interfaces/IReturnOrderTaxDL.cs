using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ReturnOrder.DL.Interfaces
{
    public interface IReturnOrderTaxDL
    {
        Task<PagedResponse<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderTax>> SelectAllReturnOrderTaxDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);

        Task<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderTax> SelectReturnOrderTaxByUID(string UID);
        Task<int> CreateReturnOrderTax(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderTax returnOrderTaxDetails);

        Task<int> UpdateReturnOrderTax(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderTax returnOrderTaxDetails);
        Task<int> DeleteReturnOrderTax(String UID);
    }
}
