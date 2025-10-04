using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.BroadClassification.DL.Interfaces
{
    public interface IBroadClassificationHeaderDL
    {
        Task<PagedResponse<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader>> GetBroadClassificationHeaderDetails(List<SortCriteria> sortCriterias, int pageNumber,
              int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader> GetBroadClassificationHeaderDetailsByUID(string UID);
        Task<int> CreateBroadClassificationHeader(Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader broadClassificationHeader);
        Task<int> UpdateBroadClassificationHeader(Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader broadClassificationHeader);
        Task<int> DeleteBroadClassificationHeader(String UID);
    }
}
