using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Modules.SKUClass.Model.UIInterfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKUClass.DL.Interfaces
{
    public interface ISKUClassGroupItemsDL
    {
        Task<PagedResponse<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems>> SelectAllSKUClassGroupItemsDetails(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems> GetSKUClassGroupItemsByUID(string UID);
        Task<int> CreateSKUClassGroupItems(Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems createSKUClassGroupItems, IDbConnection? connection = null, IDbTransaction? transaction = null);
        Task<int> CreateSKUClassGroupItemsBulk(List<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems> createSKUClassGroupItemsBulk, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            throw new NotImplementedException();
        }
        Task<int> UpdateSKUClassGroupItems(Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems updateSKUClassGroupItems, IDbConnection? connection = null, IDbTransaction? transaction = null);
        Task<int> DeleteSKUClassGroupItems(string UID);
        Task<int> DeleteSKUClassGroupItems(List<string> UIDs, IDbConnection? connection = null, IDbTransaction? transaction = null);
        Task<PagedResponse<ISKUClassGroupItemView>> SelectAllSKUClassGroupItemView(List<SortCriteria> sortCriterias, int pageNumber,
                      int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<IEnumerable<ISKUClassGroupItems>> PrepareSkuClassForCache(List<string> linkedItemUIDs)
        {
            throw new NotImplementedException();
        }

        Task<List<string>> GetApplicableAllowedSKUGroupUIDs(string storeUID)
        {
            throw new NotImplementedException();
        }
        Task<List<string>> GetApplicableAllowedSKUGBySKUClassGroupUID(string skuClassGroupUID);
    }
}
