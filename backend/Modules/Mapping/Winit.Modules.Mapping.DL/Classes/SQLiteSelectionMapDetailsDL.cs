using Microsoft.Extensions.Configuration;
using System.Text;
using Winit.Modules.Mapping.DL.Interfaces;
using Winit.Modules.Mapping.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using System.Data;

namespace Winit.Modules.Mapping.DL.Classes;

public class SQLiteSelectionMapDetailsDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, ISelectionMapDetailsDL
{
    public SQLiteSelectionMapDetailsDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider)
    {

    }

    Task<int> ISelectionMapDetailsDL.CreateSelectionMapDetails(List<ISelectionMapDetails> createSelectionMapDetails, IDbConnection? connection, IDbTransaction? transaction)
    {
        throw new NotImplementedException();
    }

    Task<int> ISelectionMapDetailsDL.DeleteSelectionMapDetails(List<string> UIDs, IDbConnection? connection, IDbTransaction? transaction)
    {
        throw new NotImplementedException();
    }

    Task<ISelectionMapDetails> ISelectionMapDetailsDL.GetSelectionMapDetailsByUID(string UID)
    {
        throw new NotImplementedException();
    }

    Task<PagedResponse<ISelectionMapDetails>> ISelectionMapDetailsDL.SelectAllSelectionMapDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        throw new NotImplementedException();
    }

    Task<int> ISelectionMapDetailsDL.UpdateSelectionMapDetails(List<ISelectionMapDetails> updateSelectionMapDetails, IDbConnection? connection, IDbTransaction? transaction)
    {
        throw new NotImplementedException();
    }
}

