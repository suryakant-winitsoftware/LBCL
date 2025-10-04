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
    public interface ISchemeExcludeMappingDL
    {
        Task<PagedResponse<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping>> SelectAllSchemeExcludeMapping(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<PagedResponse<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping>> SelectAllSchemeExcludeMappingHistory(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<List<ISchemeExcludeMapping>> GetSchemeExcludeMappings(List<SchemeKey> schemeKeys);
        Task<int> InsertSchemeExcludeMapping(List<ISchemeExcludeMapping> schemeExcludeMapping);
        Task<int> UpdateSchemeExcludeMapping(List<ISchemeExcludeMapping> schemeExcludeMapping);
        Task<int> CheckSchemeExcludeMappingExists(string storeUID, DateTime currentDate);
        Task<string?> CheckUIDExistsInDB(string TableName, string UID);
    }
}
