using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Merchandiser.Model.Interfaces;

namespace Winit.Modules.Merchandiser.BL.Interfaces
{
    public interface IProductSamplingBL
    {
        Task<IProductSampling> GetByUID(string uid);
        Task<List<IProductSampling>> GetAll();
        Task<bool> Insert(IProductSampling productSampling);
        Task<bool> Update(IProductSampling productSampling);
        Task<bool> Delete(string uid);
        Task<List<IProductSampling>> GetByStoreUID(string storeUID);
        Task<List<IProductSampling>> GetByRouteUID(string routeUID);
        Task<List<IProductSampling>> GetByEmpUID(string empUID);
        Task<bool> Validate(IProductSampling productSampling);
    }
} 