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
    public class ReturnOrderTaxBL:IReturnOrderTaxBL
    {
        protected readonly DL.Interfaces.IReturnOrderTaxDL _returnOrderTaxDL = null;
        public ReturnOrderTaxBL(DL.Interfaces.IReturnOrderTaxDL returnOrderTaxDL)
        {
            _returnOrderTaxDL = returnOrderTaxDL;
        }
        public async Task<PagedResponse<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderTax>> SelectAllReturnOrderTaxDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _returnOrderTaxDL.SelectAllReturnOrderTaxDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Model.Interfaces.IReturnOrderTax> SelectReturnOrderTaxByUID(string UID)
        {
            return await _returnOrderTaxDL.SelectReturnOrderTaxByUID(UID);
        }
        public async Task<int> CreateReturnOrderTax(Model.Interfaces.IReturnOrderTax returnOrderTax)
        {
            return await _returnOrderTaxDL.CreateReturnOrderTax(returnOrderTax);
        }
        public async Task<int> UpdateReturnOrderTax(Model.Interfaces.IReturnOrderTax ReturnOrderTax)
        {
            return await _returnOrderTaxDL.UpdateReturnOrderTax(ReturnOrderTax);
        }
        public async Task<int> DeleteReturnOrderTax(string UID)
        {
            return await _returnOrderTaxDL.DeleteReturnOrderTax(UID);
        }
    }
}
