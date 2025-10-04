using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.BroadClassification.BL.Classes
{
    public class BroadClassificationHeaderBL : Interfaces.IBroadClassificationHeaderBL
    {
        protected readonly DL.Interfaces.IBroadClassificationHeaderDL _broadClassificationHeaderDL;
        public BroadClassificationHeaderBL(DL.Interfaces.IBroadClassificationHeaderDL broadClassificationHeaderDL)
        {
            _broadClassificationHeaderDL = broadClassificationHeaderDL;   
        }
       public async Task<PagedResponse<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader>> GetBroadClassificationHeaderDetails(List<SortCriteria> sortCriterias,
           int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _broadClassificationHeaderDL.GetBroadClassificationHeaderDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
      public async  Task<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader> GetBroadClassificationHeaderDetailsByUID(string UID)
        {
            return await _broadClassificationHeaderDL.GetBroadClassificationHeaderDetailsByUID(UID);   
        }
     public async Task<int> CreateBroadClassificationHeader(Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader broadClassificationHeader)
        {
            return await _broadClassificationHeaderDL.CreateBroadClassificationHeader(broadClassificationHeader);
        }
      public async  Task<int> UpdateBroadClassificationHeader(Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader broadClassificationHeader)
        {
            return await _broadClassificationHeaderDL.UpdateBroadClassificationHeader(broadClassificationHeader);
        }
      public async  Task<int> DeleteBroadClassificationHeader(String UID)
        {
            return await _broadClassificationHeaderDL.DeleteBroadClassificationHeader(UID);
        }
    }
}
