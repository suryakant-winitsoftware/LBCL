using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Product.BL.Classes
{
    public class ProductConfigBL : ProductBaseBL, Interfaces.IProductConfigBL
    {
        protected readonly DL.Interfaces.IProductConfigDL _productConfigDL;
        public ProductConfigBL(DL.Interfaces.IProductConfigDL productConfigDL)
        {
            _productConfigDL = productConfigDL;
        }
        public async Task<PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductConfig>> SelectProductConfigAll(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _productConfigDL.SelectProductConfigAll(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async  Task<Model.Interfaces.IProductConfig> SelectProductConfigByUID(string UID)
        {
            return await _productConfigDL.SelectProductConfigByUID(UID);
        }
        public async  Task<int> CreateProductConfig(Model.Interfaces.IProductConfig createProductConfig)
        {
            return await _productConfigDL.CreateProductConfig(createProductConfig);
        }

        public async  Task<int> UpdateProductConfig(Model.Interfaces.IProductConfig UpdateProductConfig)
        {
            return await _productConfigDL.UpdateProductConfig(UpdateProductConfig);
        }

        public async  Task<int> DeleteProductConfig(string UID)
        {
            return await _productConfigDL.DeleteProductConfig(UID);
        }

        public async  Task<IEnumerable<Model.Interfaces.IProductConfig>> GetProductConfigFiltered(string ProductCode, DateTime CreatedTime, DateTime ModifiedTime)
        {
            return await _productConfigDL.GetProductConfigFiltered(ProductCode, CreatedTime, ModifiedTime);
        }

        public async  Task<IEnumerable<Model.Interfaces.IProductConfig>> GetProductConfigPaged(int pageNumber, int pageSize)
        {
            return await _productConfigDL.GetProductConfigPaged(pageNumber, pageSize);
        }

        public async  Task<IEnumerable<Model.Interfaces.IProductConfig>> GetProductConfigSorted(List<SortCriteria> sortCriterias)
        {
            return await _productConfigDL.GetProductConfigSorted(sortCriterias);
        }



    }
}
