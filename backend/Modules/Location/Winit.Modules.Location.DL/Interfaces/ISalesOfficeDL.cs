using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.DL.Interfaces
{
    public interface ISalesOfficeDL
    {
        Task<PagedResponse<Winit.Modules.Location.Model.Interfaces.ISalesOffice>> SelectAllSalesOfficeDetails(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<List<Winit.Modules.Location.Model.Interfaces.ISalesOffice>> GetSalesOfficeByUID(string UID);
        Task<int> CreateSalesOffice(Winit.Modules.Location.Model.Interfaces.ISalesOffice salesOffice);
        Task<int> UpdateSalesOffice(Winit.Modules.Location.Model.Interfaces.ISalesOffice salesOffice);
        Task<int> DeleteSalesOffice(string UID);
        Task<string?> GetWareHouseUIDbySalesOfficeUID(string salesOfficeUID);
    }
}
