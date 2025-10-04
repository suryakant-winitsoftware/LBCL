using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Product.BL.Interfaces
{
    public interface IProductTypeBL
    {
        Task<PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductType>> SelectProductTypeAll(List<SortCriteria> sortCriterias, int pageNumber,
   int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Product.Model.Interfaces.IProductType> GetProductTypeByUID(string UID);
        Task<int> CreateProductType(Winit.Modules.Product.Model.Interfaces.IProductType CreateProductType);
        Task<int> UpdateProductType(Winit.Modules.Product.Model.Interfaces.IProductType UpdateProductType);
        Task<int> DeleteProductType(string UID);
        Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductType>> GetProductTypeFiltered(string product_type_code, DateTime CreatedTime, DateTime ModifiedTime);
        Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductType>> GetProductTypePaged(int pageNumber, int pageSize);
        Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductType>> GetProductTypeSorted(List<SortCriteria> sortCriterias);
    }
}
