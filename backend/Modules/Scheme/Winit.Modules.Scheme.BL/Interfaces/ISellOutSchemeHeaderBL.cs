using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.BL.Interfaces
{
    public interface ISellOutSchemeHeaderBL
    {
        Task<PagedResponse<ISellOutSchemeHeader>> SelectAllSellOutSchemeHeader(
       List<SortCriteria> sortCriterias,
       int pageNumber,
       int pageSize,
       List<FilterCriteria> filterCriterias,
       bool isCountRequired
   );

        Task<ISellOutSchemeHeader> GetSellOutSchemeHeaderByUID(string UID);

        Task<int> CreateSellOutSchemeHeader(ISellOutSchemeHeader sellOutSchemeHeader);

        Task<int> UpdateSellOutSchemeHeader(ISellOutSchemeHeader sellOutSchemeHeader);

        Task<int> DeleteSellOutSchemeHeader(string UID);
        Task<bool> CrudSellOutMaster(ISellOutMasterScheme sellOutMasterScheme);

        Task<ISellOutMasterScheme> GetSellOutMasterByUID(string UID);
    }
}
