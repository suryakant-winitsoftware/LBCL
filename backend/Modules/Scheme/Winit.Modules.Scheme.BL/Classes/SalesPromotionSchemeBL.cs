using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.BL.Classes
{
    public class SalesPromotionSchemeBL: ISalesPromotionSchemeBL
    {
        private readonly ISalesPromotionSchemeDL _salesPromotionSchemeDL;

        public SalesPromotionSchemeBL(ISalesPromotionSchemeDL salesPromotionSchemeDL)
        {
            _salesPromotionSchemeDL = salesPromotionSchemeDL;
        }

        public async Task<PagedResponse<ISalesPromotionScheme>> SelectAllSalesPromotionScheme(
            List<SortCriteria> sortCriterias,
            int pageNumber,
            int pageSize,
            List<FilterCriteria> filterCriterias,
            bool isCountRequired)
        {
            return await _salesPromotionSchemeDL.SelectAllSalesPromotionScheme(
                sortCriterias,
                pageNumber,
                pageSize,
                filterCriterias,
                isCountRequired
            );
        }

        public async Task<ISalesPromotionScheme> GetSalesPromotionSchemeByUID(string UID)
        {
            return await _salesPromotionSchemeDL.GetSalesPromotionSchemeByUID(UID);
        }

        public async Task<int> CreateSalesPromotionScheme(SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO)
        {
            return await _salesPromotionSchemeDL.CreateSalesPromotionScheme(salesPromotionSchemeApprovalDTO);
        }

        public async Task<int> UpdateSalesPromotionScheme(SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO)
        {
            return await _salesPromotionSchemeDL.UpdateSalesPromotionScheme(salesPromotionSchemeApprovalDTO);
        }

        public async Task<int> DeleteSalesPromotionScheme(string UID)
        {
            return await _salesPromotionSchemeDL.DeleteSalesPromotionScheme(UID);
        }
    }
}
