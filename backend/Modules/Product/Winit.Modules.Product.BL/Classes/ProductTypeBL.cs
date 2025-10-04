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
    public class ProductTypeBL:IProductTypeBL
    {
        protected readonly DL.Interfaces.IProductTypeDL _productTypeRepository = null;
        public ProductTypeBL(DL.Interfaces.IProductTypeDL productTypeRepository)
        {
            _productTypeRepository = productTypeRepository;
        }
        public async  Task<PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductType>> SelectProductTypeAll(List<SortCriteria> sortCriterias, int pageNumber,
                          int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _productTypeRepository.SelectProductTypeAll(sortCriterias,pageNumber,pageSize,filterCriterias, isCountRequired);
        }
        public async  Task<Winit.Modules.Product.Model.Interfaces.IProductType> GetProductTypeByUID(string UID)
        {
            return await _productTypeRepository.GetProductTypeByUID(UID);
        }
        public async  Task<int> CreateProductType(Winit.Modules.Product.Model.Interfaces.IProductType CreateProductType)
        {
            return await _productTypeRepository.CreateProductType(CreateProductType);
        }

        public async  Task<int> UpdateProductType(Winit.Modules.Product.Model.Interfaces.IProductType UpdateProductType)
        {
            return await _productTypeRepository.UpdateProductType(UpdateProductType);
        }

        public async  Task<int> DeleteProductType(string UID)
        {
            return await _productTypeRepository.DeleteProductType(UID);
        }

        public async  Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductType>> GetProductTypeFiltered(string product_type_code, DateTime CreatedTime, DateTime ModifiedTime)
        {
            return await _productTypeRepository.GetProductTypeFiltered(product_type_code, CreatedTime, ModifiedTime);
        }

        public async  Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductType>> GetProductTypePaged(int pageNumber, int pageSize)
        {
            return await _productTypeRepository.GetProductTypePaged(pageNumber, pageSize);
        }

        public async  Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductType>> GetProductTypeSorted(List<SortCriteria> sortCriterias)
        {
            return await _productTypeRepository.GetProductTypeSorted(sortCriterias);
        }
    }
}
