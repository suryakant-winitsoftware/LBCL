using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ReturnOrder.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ReturnOrder.BL.Classes
{
    public class ReturnOrderLineBL:IReturnOrderLineBL
    {
        protected readonly DL.Interfaces.IReturnOrderLineDL _returnOrderLineDL = null;
        public ReturnOrderLineBL(DL.Interfaces.IReturnOrderLineDL returnOrderLineDL)
        {
            _returnOrderLineDL = returnOrderLineDL;
        }
        public async Task<PagedResponse<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine>> SelectAllReturnOrderLineDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _returnOrderLineDL.SelectAllReturnOrderLineDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Model.Interfaces.IReturnOrderLine> SelectReturnOrderLineByUID(string UID)
        {
            return await _returnOrderLineDL.SelectReturnOrderLineByUID(UID);
        }
        public async Task<int> CreateReturnOrderLine(Model.Interfaces.IReturnOrderLine returnOrderLine)
        {
            return await _returnOrderLineDL.CreateReturnOrderLine(returnOrderLine);
        }
        public async Task<int> UpdateReturnOrderLine(Model.Interfaces.IReturnOrderLine ReturnOrderLine)
        {
            return await _returnOrderLineDL.UpdateReturnOrderLine(ReturnOrderLine);
        }
        public async Task<int> DeleteReturnOrderLine(string UID)
        {
            return await _returnOrderLineDL.DeleteReturnOrderLine(UID);
        }
    }
}
