using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKUClass.DL.Interfaces;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Modules.SKUClass.Model.UIInterfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKUClass.DL.Classes;

public class SQLiteSKUClassGroupItemsDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, ISKUClassGroupItemsDL
{
    public SQLiteSKUClassGroupItemsDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider)
    {

    }
    
    public async Task<List<string>> GetApplicableAllowedSKUGBySKUClassGroupUID(string skuClassGroupUID)
    {
        string sql = """
                       SELECT sku_uid FROM sku_class_group_items where sku_class_group_uid = @SKUClassGroupUID
            """;
        var parameters = new Dictionary<string, object>
        {
            {"SKUClassGroupUID",skuClassGroupUID }
        };
        List<string> sKUClassGroupItemss =
                await ExecuteQueryAsync<string>(sql.ToString(), parameters);

        return sKUClassGroupItemss;
    }

    Task<int> ISKUClassGroupItemsDL.CreateSKUClassGroupItems(ISKUClassGroupItems createSKUClassGroupItems, IDbConnection? connection, IDbTransaction? transaction)
    {
        throw new NotImplementedException();
    }

    Task<int> ISKUClassGroupItemsDL.DeleteSKUClassGroupItems(string UID)
    {
        throw new NotImplementedException();
    }

    Task<int> ISKUClassGroupItemsDL.DeleteSKUClassGroupItems(List<string> UIDs, IDbConnection? connection, IDbTransaction? transaction)
    {
        throw new NotImplementedException();
    }

    Task<ISKUClassGroupItems> ISKUClassGroupItemsDL.GetSKUClassGroupItemsByUID(string UID)
    {
        throw new NotImplementedException();
    }

    Task<PagedResponse<ISKUClassGroupItems>> ISKUClassGroupItemsDL.SelectAllSKUClassGroupItemsDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        throw new NotImplementedException();
    }

    Task<PagedResponse<ISKUClassGroupItemView>> ISKUClassGroupItemsDL.SelectAllSKUClassGroupItemView(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        throw new NotImplementedException();
    }

    Task<int> ISKUClassGroupItemsDL.UpdateSKUClassGroupItems(ISKUClassGroupItems updateSKUClassGroupItems, IDbConnection? connection, IDbTransaction? transaction)
    {
        throw new NotImplementedException();
    }
}



