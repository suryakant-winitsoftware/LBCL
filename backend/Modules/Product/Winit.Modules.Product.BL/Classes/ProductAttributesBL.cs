using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Product.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Product.BL.Classes
{
    public  class ProductAttributesBL:IProductAttributesBL
    {
        protected readonly DL.Interfaces.IProductAttributesDL _productAttributesDL = null;
        public ProductAttributesBL(DL.Interfaces.IProductAttributesDL productAttributesDL)
        {
            _productAttributesDL = productAttributesDL;
        }
        public async Task<PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductAttributes>> SelectProductAttributesAll(List<SortCriteria> sortCriterias, int pageNumber,
   int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _productAttributesDL.SelectProductAttributesAll(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
    }
}
