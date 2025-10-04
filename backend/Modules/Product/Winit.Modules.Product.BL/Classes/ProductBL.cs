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
    public class ProductBL : ProductBaseBL, Interfaces.IProductBL
    {
        protected readonly DL.Interfaces.IProductDL _productDL;
        public ProductBL(DL.Interfaces.IProductDL productDL)
        {
            _productDL = productDL;
        }
       
        public async Task<PagedResponse<Model.Interfaces.IProduct>> SelectProductsAll(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _productDL.SelectProductsAll(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }

        public async Task<Model.Interfaces.IProduct> SelectProductByUID(string UID)
        {
            return await _productDL.SelectProductByUID(UID);
        }
        public async  Task<int> CreateProduct(Model.Interfaces.IProduct CreateProduct)
        {
            return await _productDL.CreateProduct(CreateProduct);

        }
        public async Task<int> UpdateProduct(Model.Interfaces.IProduct UpdateProduct)
        {
            return await _productDL.UpdateProduct(UpdateProduct);
        }
        public async Task<int> DeleteProduct(string productCode)
        {
            return await _productDL.DeleteProduct(productCode);
        }
        public async Task<IEnumerable<Model.Interfaces.IProduct>> GetProductsFiltered(string product_code, String product_name, DateTime CreatedTime, DateTime ModifiedTime)
        {
            return await _productDL.GetProductsFiltered(product_code, product_name, CreatedTime, ModifiedTime);
        }

        public async Task<IEnumerable<Model.Interfaces.IProduct>> GetProductsPaged(int pageNumber, int pageSize)
        {
            return await _productDL.GetProductsPaged(pageNumber, pageSize);
        }

       
        public async Task<IEnumerable<Model.Interfaces.IProduct>> GetProductsSorted(List<SortCriteria> sortCriterias)
        {
            return await _productDL.GetProductsSorted(sortCriterias);
        }
        public async Task<IEnumerable<Model.Interfaces.IProduct>> SelectAllProduct(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias)
        {
            return await _productDL.SelectAllProduct(sortCriterias,pageNumber,pageSize,filterCriterias);
        }



    }
}
