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
    public class SellOutSchemeLineBL: ISellOutSchemeLineBL
    {
        private readonly ISellOutSchemeLineDL _sellOutSchemeLineDL;

        public SellOutSchemeLineBL(ISellOutSchemeLineDL sellOutSchemeLineDL)
        {
            _sellOutSchemeLineDL = sellOutSchemeLineDL;
        }

        public async Task<PagedResponse<ISellOutSchemeLine>> SelectAllSellOutSchemeLine(
            List<SortCriteria> sortCriterias,
            int pageNumber,
            int pageSize,
            List<FilterCriteria> filterCriterias,
            bool isCountRequired)
        {
            return await _sellOutSchemeLineDL.SelectAllSellOutSchemeLine(
                sortCriterias,
                pageNumber,
                pageSize,
                filterCriterias,
                isCountRequired
            );
        }

        public async Task<ISellOutSchemeLine> GetSellOutSchemeLineByUID(string UID)
        {
            return await _sellOutSchemeLineDL.GetSellOutSchemeLineByUID(UID);
        }

        public async Task<int> CreateSellOutSchemeLine(ISellOutSchemeLine sellOutSchemeLine)
        {
            return await _sellOutSchemeLineDL.CreateSellOutSchemeLine(sellOutSchemeLine);
        }

        public async Task<int> UpdateSellOutSchemeLine(ISellOutSchemeLine sellOutSchemeLine)
        {
            return await _sellOutSchemeLineDL.UpdateSellOutSchemeLine(sellOutSchemeLine);
        }

        public async Task<int> DeleteSellOutSchemeLine(string UID)
        {
            return await _sellOutSchemeLineDL.DeleteSellOutSchemeLine(UID);
        }

        public async Task<List<IPreviousOrders>> GetPreviousOrdersByChannelPartnerUID(string UID)
        {
            return await _sellOutSchemeLineDL.GetPreviousOrdersByChannelPartnerUID(UID);
        }
    }
}
