using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Product.BL.Interfaces
{
    public interface IProductConfigBL
    {

       Task<PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductConfig>> SelectProductConfigAll(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);

        Task<Winit.Modules.Product.Model.Interfaces.IProductConfig> SelectProductConfigByUID(string UID);
        Task<int> CreateProductConfig(Winit.Modules.Product.Model.Interfaces.IProductConfig createProductConfig);

        Task<int> UpdateProductConfig(Winit.Modules.Product.Model.Interfaces.IProductConfig updateProductConfig);
        Task<int> DeleteProductConfig(string UID);
        Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductConfig>> GetProductConfigFiltered(string ProductCode, DateTime CreatedTime, DateTime ModifiedTime);
        Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductConfig>> GetProductConfigPaged(int pageNumber, int pageSize);

        Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductConfig>> GetProductConfigSorted(List<SortCriteria> sortCriterias);

    }
}
