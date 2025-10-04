using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.DL.Interfaces
{
    public interface ISellOutSchemeLineDL
    {
        Task<PagedResponse<ISellOutSchemeLine>> SelectAllSellOutSchemeLine(
        List<SortCriteria> sortCriterias,
        int pageNumber,
        int pageSize,
        List<FilterCriteria> filterCriterias,
        bool isCountRequired);

        Task<ISellOutSchemeLine> GetSellOutSchemeLineByUID(string UID);
        Task<List<ISellOutSchemeLine>> GetSellOutSchemeLinesByUIDs(List<string> UIDs);

        Task<int> CreateSellOutSchemeLine(ISellOutSchemeLine sellOutSchemeLine);

        Task<int> UpdateSellOutSchemeLine(ISellOutSchemeLine sellOutSchemeLine);

        Task<int> DeleteSellOutSchemeLine(string UID);
        Task<int> CreateSellOutLines(List<ISellOutSchemeLine> sellOutSchemeLines, IDbConnection? dbConnection = null,
       IDbTransaction? dbTransaction = null);
        Task<int> UpdateSellOutSchemeLines(List<ISellOutSchemeLine> sellOutSchemeLines, IDbConnection? dbConnection = null, IDbTransaction? dbTransaction = null);
        Task<List<IPreviousOrders>> GetPreviousOrdersByChannelPartnerUID(string UID);
    }
}
