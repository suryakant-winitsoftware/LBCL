using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.BL.Interfaces
{
    public interface ISchemeExcludeMappingBL
    {
        Task<string?> CheckIfUIDExistsInDB(string TableName, string UID);
        Task<List<SchemeExcludeMapping>> BulkImport(List<SchemeExcludeMapping> schemeExcludeMappings);
        Task<int> CheckSchemeExcludeMappingExists(string storeUID, DateTime currentDate);
        Task<PagedResponse<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping>> SelectAllSchemeExcludeMapping(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<PagedResponse<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping>> SelectAllSchemeExcludeMappingHistory(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
    }
}
