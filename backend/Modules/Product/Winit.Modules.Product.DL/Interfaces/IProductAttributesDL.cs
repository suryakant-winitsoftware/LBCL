using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Product.DL.Interfaces
{
    public interface IProductAttributesDL
    {
        Task<PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductAttributes>> SelectProductAttributesAll(List<SortCriteria> sortCriterias, int pageNumber,
   int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
    }
}
