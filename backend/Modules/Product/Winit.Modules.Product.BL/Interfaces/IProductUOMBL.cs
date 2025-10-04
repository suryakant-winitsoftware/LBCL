using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Product.BL.Interfaces
{
    public interface IProductUOMBL
    {
        Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductUOM>> SelectProductUOMAll();
        Task<Winit.Modules.Product.Model.Interfaces.IProductUOM> GetProductUOMByUID(string UID);
        Task<int> CreateProductUOM(Winit.Modules.Product.Model.Interfaces.IProductUOM CreateProductUOM);
        Task<int> UpdateProductUOM(Winit.Modules.Product.Model.Interfaces.IProductUOM UpdateProductUOM);
        Task<int> DeleteProductUOM(string UID);
        Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductUOM>> GetProductUOMFiltered(string ProductCode, DateTime CreatedTime, DateTime ModifiedTime);
        Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductUOM>> GetProductUOMPaged(int pageNumber, int pageSize);
        Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductUOM>> GetProductUOMSorted(List<SortCriteria> sortCriterias);
        Task<PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductUOM>> SelectAllProductUOM(List<SortCriteria> sortCriterias, int pageNumber, int pageSize,List<FilterCriteria> filterCriterias, bool isCountRequired);
    }
}
