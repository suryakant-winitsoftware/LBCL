using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.DL.Interfaces
{
    public interface ISalesPromotionSchemeDL
    {
        Task<PagedResponse<ISalesPromotionScheme>> SelectAllSalesPromotionScheme(
       List<SortCriteria> sortCriterias,
       int pageNumber,
       int pageSize,
       List<FilterCriteria> filterCriterias,
       bool isCountRequired);

        Task<ISalesPromotionScheme> GetSalesPromotionSchemeByUID(string UID);

        Task<int> CreateSalesPromotionScheme(SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO);

        Task<int> UpdateSalesPromotionScheme(SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO);

        Task<int> DeleteSalesPromotionScheme(string UID);
    }
}
