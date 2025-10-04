using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Product.BL.Interfaces
{
    public interface IProductBL
    {
        Task<PagedResponse<Model.Interfaces.IProduct>> SelectProductsAll(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Model.Interfaces.IProduct> SelectProductByUID(string UID);
        Task<int> CreateProduct(Model.Interfaces.IProduct Product);
        Task<int> UpdateProduct(Model.Interfaces.IProduct UpdateProduct);
        Task<int> DeleteProduct(string productCode);
        Task<IEnumerable<Model.Interfaces.IProduct>> GetProductsFiltered(string product_code, string product_name, DateTime CreatedTime, DateTime ModifiedTime);
        Task<IEnumerable<Model.Interfaces.IProduct>> GetProductsPaged(int pageNumber, int pageSize);
        Task<IEnumerable<Model.Interfaces.IProduct>> GetProductsSorted(List<SortCriteria> sortCriterias);
        Task<IEnumerable<Model.Interfaces.IProduct>> SelectAllProduct(List<SortCriteria> sortCriterias, int pageNumber, int pageSize,List<FilterCriteria> filterCriterias);

    }
}
