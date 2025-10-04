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
    public interface ISellOutSchemeLineBL
    {
        Task<PagedResponse<ISellOutSchemeLine>> SelectAllSellOutSchemeLine(
        List<SortCriteria> sortCriterias,
        int pageNumber,
        int pageSize,
        List<FilterCriteria> filterCriterias,
        bool isCountRequired);

        Task<ISellOutSchemeLine> GetSellOutSchemeLineByUID(string UID);

        Task<int> CreateSellOutSchemeLine(ISellOutSchemeLine sellOutSchemeLine);

        Task<int> UpdateSellOutSchemeLine(ISellOutSchemeLine sellOutSchemeLine);

        Task<int> DeleteSellOutSchemeLine(string UID);

        Task<List<IPreviousOrders>> GetPreviousOrdersByChannelPartnerUID(string UID);
    }
}
