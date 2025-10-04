using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Merchandiser.Model.Interfaces;

namespace Winit.Modules.Merchandiser.BL.Interfaces
{
    public interface IProductFeedbackBL
    {
        Task<IProductFeedback> GetByUID(string uid);
        Task<List<IProductFeedback>> GetAll();
        Task<bool> Insert(IProductFeedback productFeedback);
        Task<bool> Save(IProductFeedback productFeedback);
        Task<bool> Delete(string uid);
        Task<List<IProductFeedback>> GetByStoreUID(string storeUID);
        Task<List<IProductFeedback>> GetByRouteUID(string routeUID);
        Task<List<IProductFeedback>> GetByEmpUID(string empUID);
    }
} 