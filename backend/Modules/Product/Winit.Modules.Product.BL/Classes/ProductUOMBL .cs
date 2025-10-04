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
    public class ProductUOMBL : ProductBaseBL, Interfaces.IProductUOMBL
    {
        protected readonly DL.Interfaces.IProductUOMDL _productUOMDL;
        public ProductUOMBL(DL.Interfaces.IProductUOMDL productUOMDL)
        {
            _productUOMDL = productUOMDL;
        }
        public async  Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductUOM>> SelectProductUOMAll()
        {
            return await _productUOMDL.SelectProductUOMAll();
        }
        public async  Task<Winit.Modules.Product.Model.Interfaces.IProductUOM> GetProductUOMByUID(string UID)
        {
            return await _productUOMDL.GetProductUOMByUID(UID);
        }
        public async  Task<int> CreateProductUOM(Winit.Modules.Product.Model.Interfaces.IProductUOM CreateProductUOM)
        {
            return await _productUOMDL.CreateProductUOM(CreateProductUOM);
        }

        public async  Task<int> UpdateProductUOM(Winit.Modules.Product.Model.Interfaces.IProductUOM UpdateProductUOM)
        {
            return await _productUOMDL.UpdateProductUOM(UpdateProductUOM);
        }

        public async  Task<int> DeleteProductUOM(string UID)
        {
            return await _productUOMDL.DeleteProductUOM(UID);
        }

        public async  Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductUOM>> GetProductUOMFiltered(string ProductCode, DateTime CreatedTime, DateTime ModifiedTime)
        {
            return await _productUOMDL.GetProductUOMFiltered(ProductCode, CreatedTime, ModifiedTime);
        }

        public async  Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductUOM>> GetProductUOMPaged(int pageNumber, int pageSize)
        {
            return await _productUOMDL.GetProductUOMPaged(pageNumber, pageSize);
        }

        public async  Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductUOM>> GetProductUOMSorted(List<SortCriteria> sortCriterias)
        {
            return await _productUOMDL.GetProductUOMSorted(sortCriterias);
        }
        
        public async  Task<PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductUOM>> SelectAllProductUOM(List<SortCriteria> sortCriterias,
            int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _productUOMDL.SelectAllProductUOM(sortCriterias,pageNumber,pageSize,filterCriterias, isCountRequired);
        }
    }
}
