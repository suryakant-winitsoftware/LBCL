using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Merchandiser.Model.Interfaces;

namespace Winit.Modules.Merchandiser.DL.Interfaces
{
    public interface IProductFeedbackDL
    {
        Task<IProductFeedback> GetByUID(string uid);
        Task<List<IProductFeedback>> GetAll();
        Task<bool> Insert(IProductFeedback productFeedback);
        Task<bool> Update(IProductFeedback productFeedback);
        Task<bool> Delete(string uid);
        Task<List<IProductFeedback>> GetByStoreUID(string storeUID);
        Task<List<IProductFeedback>> GetByRouteUID(string routeUID);
        Task<List<IProductFeedback>> GetByEmpUID(string empUID);
    }
} 