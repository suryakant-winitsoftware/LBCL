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
    public class ProductDimensionBL : ProductBaseBL, Interfaces.IProductDimensionBL
    {
        protected readonly DL.Interfaces.IProductDimensionDL _productDimensionDL;
        public ProductDimensionBL(DL.Interfaces.IProductDimensionDL productDimensionDL)
        {
            _productDimensionDL = productDimensionDL;
        }
        public async  Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductDimension>> SelectProductDimensionAll()
        {
            return await _productDimensionDL.SelectProductDimensionAll();
        }
        public async  Task<Winit.Modules.Product.Model.Interfaces.IProductDimension> GetProductDimensionByUID(string UID)
        {
            return await _productDimensionDL.GetProductDimensionByUID(UID);
        }
        public async  Task<int> CreateProductDimension(Winit.Modules.Product.Model.Interfaces.IProductDimension CreateProductDimension)
        {
            return await _productDimensionDL.CreateProductDimension(CreateProductDimension);
        }

        public async  Task<int> UpdateProductDimension(Winit.Modules.Product.Model.Interfaces.IProductDimension UpdateProductDimension)
        {
            return await _productDimensionDL.UpdateProductDimension(UpdateProductDimension);
        }

        public async  Task<int> DeleteProductDimension(string UID)
        {
            return await _productDimensionDL.DeleteProductDimension(UID);
        }

        public async  Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductDimension>> GetProductDimensionFiltered(string product_dimension_code, DateTime CreatedTime, DateTime ModifiedTime)
        {
            return await _productDimensionDL.GetProductDimensionFiltered(product_dimension_code, CreatedTime, ModifiedTime);
        }

        public async  Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductDimension>> GetProductDimensionPaged(int pageNumber, int pageSize)
        {
            return await _productDimensionDL.GetProductDimensionPaged(pageNumber, pageSize);
        }

        public async  Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductDimension>> GetProductDimensionSorted(List<SortCriteria> sortCriterias)
        {
            return await _productDimensionDL.GetProductDimensionSorted(sortCriterias);
        }
        public async Task<PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductDimension>> SelectAllProductDimension(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _productDimensionDL.SelectAllProductDimension(sortCriterias,pageNumber, pageSize, filterCriterias, isCountRequired);
        }
    }
}
