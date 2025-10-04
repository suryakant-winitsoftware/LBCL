using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Mapping.BL.Interfaces
{
    public interface ISelectionMapDetailsBL
    {
        Task<PagedResponse<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapDetails>> SelectAllSelectionMapDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapDetails> GetSelectionMapDetailsByUID(string UID);
        Task<int> CreateSelectionMapDetails(List<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapDetails> createSelectionMapDetails);
        Task<int> UpdateSelectionMapDetails(Winit.Modules.Mapping.Model.Interfaces.ISelectionMapDetails updateSelectionMapDetails);
        Task<int> DeleteSelectionMapDetails(List<string> UIDs);
    }
}
