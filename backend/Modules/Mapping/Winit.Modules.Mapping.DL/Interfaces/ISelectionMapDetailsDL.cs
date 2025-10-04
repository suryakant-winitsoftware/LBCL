using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Mapping.DL.Interfaces
{
    public interface ISelectionMapDetailsDL
    {
        Task<PagedResponse<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapDetails>> SelectAllSelectionMapDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapDetails> GetSelectionMapDetailsByUID(string UID);
        Task<int> CreateSelectionMapDetails(List<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapDetails> createSelectionMapDetails, IDbConnection? connection = null, IDbTransaction? transaction = null);
        Task<int> UpdateSelectionMapDetails(List<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapDetails> updateSelectionMapDetails, IDbConnection? connection = null, IDbTransaction? transaction = null);
        Task<int> DeleteSelectionMapDetails(List<string> UIDs, IDbConnection? connection = null, IDbTransaction? transaction = null);
    }
}
