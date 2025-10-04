using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.UOM.DL.Interfaces
{
    public interface IUOMTypeDL
    {
        Task<PagedResponse<Winit.Modules.UOM.Model.Interfaces.IUOMType>> SelectAllUOMTypeDetails(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
       
    }
}
