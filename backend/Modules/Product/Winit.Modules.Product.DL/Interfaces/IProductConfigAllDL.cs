using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Product.DL.Interfaces
{
    public interface IProductConfigAllDL
    {
        Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductConfigAll>> GetProductConfigAll();

        Task<Winit.Modules.Product.Model.Interfaces.IProductConfigAll> GetProductConfigByskuConfigId(int skuConfigId);
        Task<Winit.Modules.Product.Model.Interfaces.IProductConfigAll> CreateProductConfig(Winit.Modules.Product.Model.Interfaces.IProductConfigAll CreateProductConfig);

        Task<int> UpdateProductConfig(Winit.Modules.Product.Model.Interfaces.IProductConfigAll UpdateProductConfig);

        Task<int> DeleteProductConfig(int skuConfigId);
        Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductConfigAll>> GetProductConfigFiltered(string ProductCode, DateTime CreatedTime, DateTime ModifiedTime);
        Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductConfigAll>> GetProductConfigPaged(int pageNumber, int pageSize);

        Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductConfigAll>> GetProductConfigSorted(List<SortCriteria> sortCriterias);

    }
}
