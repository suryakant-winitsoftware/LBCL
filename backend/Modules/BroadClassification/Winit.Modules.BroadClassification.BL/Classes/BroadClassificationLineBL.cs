using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.BroadClassification.BL.Interfaces;
using Winit.Modules.BroadClassification.DL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.BroadClassification.BL.Classes
{

    public class BroadClassificationLineBL:IBroadClassificationLineBL
    {
        private readonly IBroadClassificationLineDL _broadClassificationLineDL;
        public BroadClassificationLineBL(IBroadClassificationLineDL broadClassificationLineDL) 
        {
            _broadClassificationLineDL= broadClassificationLineDL;
        }
       public async  Task<PagedResponse<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationLine>> GetBroadClassificationLineDetails(List<SortCriteria> sortCriterias, int pageNumber,
              int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _broadClassificationLineDL.GetBroadClassificationLineDetails(sortCriterias,pageNumber, pageSize,filterCriterias,isCountRequired);
        }
        public async Task<List<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationLine>> GetBroadClassificationLineDetailsByUID(string UID)
        {
            return await _broadClassificationLineDL.GetBroadClassificationLineDetailsByUID(UID);
        }
       public async Task<int> CreateBroadClassificationLine(Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationLine broadClassificationline)
        {
            return await _broadClassificationLineDL.CreateBroadClassificationLine(broadClassificationline);
        }
       public async Task<int> UpdateBroadClassificationLine(Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationLine broadClassificationline)
        {
            return await _broadClassificationLineDL.UpdateBroadClassificationLine(broadClassificationline);
        }
       public async Task<int> DeleteBroadClassificationLine(String UID)
        {
            return await _broadClassificationLineDL.DeleteBroadClassificationLine(UID);
        }
    }
}
