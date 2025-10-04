using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Product.BL.Classes
{
    public class ProductConfigAllBL : ProductBaseBL, Interfaces.IProductConfigAllBL
    {
        protected readonly DL.Interfaces.IProductConfigAllDL _productConfigAllRepository;
        public ProductConfigAllBL(DL.Interfaces.IProductConfigAllDL productConfigAllRepository)
        {
            _productConfigAllRepository = productConfigAllRepository;
        }
        public async  Task<IEnumerable<Model.Interfaces.IProductConfigAll>> GetProductConfigAll()
        {
            return await _productConfigAllRepository.GetProductConfigAll();
        }
        public async  Task<Model.Interfaces.IProductConfigAll> GetProductConfigByskuConfigId(int skuConfigId)
        {
            return await _productConfigAllRepository.GetProductConfigByskuConfigId(skuConfigId);
        }
        public async  Task<Model.Interfaces.IProductConfigAll> CreateProductConfig(Model.Interfaces.IProductConfigAll CreateProductConfig)
        {
            return await _productConfigAllRepository.CreateProductConfig(CreateProductConfig);
        }

        public async  Task<int> UpdateProductConfig(Model.Interfaces.IProductConfigAll UpdateProductConfig)
        {
            return await _productConfigAllRepository.UpdateProductConfig(UpdateProductConfig);
        }

        public async  Task<int> DeleteProductConfig(int skuConfigId)
        {
            return await _productConfigAllRepository.DeleteProductConfig(skuConfigId);
        }

        public async  Task<IEnumerable<Model.Interfaces.IProductConfigAll>> GetProductConfigFiltered(string ProductCode, DateTime CreatedTime, DateTime ModifiedTime)
        {
            return await _productConfigAllRepository.GetProductConfigFiltered(ProductCode, CreatedTime, ModifiedTime);
        }

        public async  Task<IEnumerable<Model.Interfaces.IProductConfigAll>> GetProductConfigPaged(int pageNumber, int pageSize)
        {
            return await _productConfigAllRepository.GetProductConfigPaged(pageNumber, pageSize);
        }

        public async  Task<IEnumerable<Model.Interfaces.IProductConfigAll>> GetProductConfigSorted(List<SortCriteria> sortCriterias)
        {
            return await _productConfigAllRepository.GetProductConfigSorted(sortCriterias);
        }



    }
}
