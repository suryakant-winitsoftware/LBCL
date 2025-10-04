using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.BroadClassification.DL.Interfaces
{
    public interface IBroadClassificationLineDL
    {
        Task<PagedResponse<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationLine>> GetBroadClassificationLineDetails(List<SortCriteria> sortCriterias, int pageNumber,
              int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<List<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationLine>> GetBroadClassificationLineDetailsByUID(string UID);
        Task<int> CreateBroadClassificationLine(Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationLine broadClassificationLine);
        Task<int> UpdateBroadClassificationLine(Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationLine broadClassificationLine);
        Task<int> DeleteBroadClassificationLine(String UID);
    }
}
