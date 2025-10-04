using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.BL.Classes
{
    public class SellOutSchemeHeaderBL: ISellOutSchemeHeaderBL
    {
        private readonly ISellOutSchemeHeaderDL _sellOutSchemeHeaderDL;

        public SellOutSchemeHeaderBL(ISellOutSchemeHeaderDL sellOutSchemeHeaderDL)
        {
            _sellOutSchemeHeaderDL = sellOutSchemeHeaderDL;
        }

        public async Task<PagedResponse<ISellOutSchemeHeader>> SelectAllSellOutSchemeHeader(
            List<SortCriteria> sortCriterias,
            int pageNumber,
            int pageSize,
            List<FilterCriteria> filterCriterias,
            bool isCountRequired)
        {
            return await _sellOutSchemeHeaderDL.SelectAllSellOutSchemeHeader(
                sortCriterias,
                pageNumber,
                pageSize,
                filterCriterias,
                isCountRequired
            );
        }

        public async Task<ISellOutSchemeHeader> GetSellOutSchemeHeaderByUID(string UID)
        {
            return await _sellOutSchemeHeaderDL.GetSellOutSchemeHeaderByUID(UID);
        }

        public async Task<int> CreateSellOutSchemeHeader(ISellOutSchemeHeader sellOutSchemeHeader)
        {
            return await _sellOutSchemeHeaderDL.CreateSellOutSchemeHeader(sellOutSchemeHeader);
        }

        public async Task<int> UpdateSellOutSchemeHeader(ISellOutSchemeHeader sellOutSchemeHeader)
        {
            return await _sellOutSchemeHeaderDL.UpdateSellOutSchemeHeader(sellOutSchemeHeader);
        }

        public async Task<int> DeleteSellOutSchemeHeader(string UID)
        {
            return await _sellOutSchemeHeaderDL.DeleteSellOutSchemeHeader(UID);
        }

        public async Task<bool> CrudSellOutMaster(ISellOutMasterScheme sellOutMasterScheme)
        {
            return await _sellOutSchemeHeaderDL.CrudSellOutMaster(sellOutMasterScheme);
        }

        public async Task<ISellOutMasterScheme> GetSellOutMasterByUID(string UID)
        {
            return await _sellOutSchemeHeaderDL.GetSellOutMasterByUID(UID);
        }
    }
}
