using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Product.BL.Interfaces
{
    public interface IProductDimensionBL
    {
        Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductDimension>> SelectProductDimensionAll();
        Task<Winit.Modules.Product.Model.Interfaces.IProductDimension> GetProductDimensionByUID(string UID);
        Task<int> CreateProductDimension(Winit.Modules.Product.Model.Interfaces.IProductDimension CreateProductDimension);
        Task<int> UpdateProductDimension(Winit.Modules.Product.Model.Interfaces.IProductDimension UpdateProductDimension);
        Task<int> DeleteProductDimension(string UID);
        Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductDimension>> GetProductDimensionFiltered(string product_dimension_code, DateTime CreatedTime, DateTime ModifiedTime);
        Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductDimension>> GetProductDimensionPaged(int pageNumber, int pageSize);
        Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductDimension>> GetProductDimensionSorted(List<SortCriteria> sortCriterias);
        Task<PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductDimension>> SelectAllProductDimension(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
    }
}
