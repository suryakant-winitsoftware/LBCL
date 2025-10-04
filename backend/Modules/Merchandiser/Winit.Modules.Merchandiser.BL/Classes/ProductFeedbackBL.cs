using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Merchandiser.BL.Interfaces;
using Winit.Modules.Merchandiser.DL.Interfaces;
using Winit.Modules.Merchandiser.Model.Interfaces;

namespace Winit.Modules.Merchandiser.BL.Classes
{
    public class ProductFeedbackBL : IProductFeedbackBL
    {
        private readonly IProductFeedbackDL _productFeedbackDL;

        public ProductFeedbackBL(IProductFeedbackDL productFeedbackDL)
        {
            _productFeedbackDL = productFeedbackDL;
        }

        public async Task<IProductFeedback> GetByUID(string uid)
        {
            if (string.IsNullOrEmpty(uid))
                throw new ArgumentNullException(nameof(uid));

            return await _productFeedbackDL.GetByUID(uid);
        }

        public async Task<List<IProductFeedback>> GetAll()
        {
            return await _productFeedbackDL.GetAll();
        }

        public async Task<bool> Insert(IProductFeedback productFeedback)
        {
            if (productFeedback == null)
                throw new ArgumentNullException(nameof(productFeedback));

            ValidateProductFeedback(productFeedback);

            //var existing = await _productFeedbackDL.GetByUID(productFeedback.UID);
            //if (existing != null)
            //    throw new InvalidOperationException($"Product Feedback with UID {productFeedback.UID} already exists");

            return await _productFeedbackDL.Insert(productFeedback);
        }

        public async Task<bool> Save(IProductFeedback productFeedback)
        {
            if (productFeedback == null)
                throw new ArgumentNullException(nameof(productFeedback));

            ValidateProductFeedback(productFeedback);

            var existing = await _productFeedbackDL.GetByUID(productFeedback.UID);
            if (existing == null)
                throw new InvalidOperationException($"Product Feedback with UID {productFeedback.UID} does not exist");

            return await _productFeedbackDL.Update(productFeedback);
        }

        public async Task<bool> Delete(string uid)
        {
            if (string.IsNullOrEmpty(uid))
                throw new ArgumentNullException(nameof(uid));

            return await _productFeedbackDL.Delete(uid);
        }

        public async Task<List<IProductFeedback>> GetByStoreUID(string storeUID)
        {
            if (string.IsNullOrEmpty(storeUID))
                throw new ArgumentNullException(nameof(storeUID));

            return await _productFeedbackDL.GetByStoreUID(storeUID);
        }

        public async Task<List<IProductFeedback>> GetByRouteUID(string routeUID)
        {
            if (string.IsNullOrEmpty(routeUID))
                throw new ArgumentNullException(nameof(routeUID));

            return await _productFeedbackDL.GetByRouteUID(routeUID);
        }

        public async Task<List<IProductFeedback>> GetByEmpUID(string empUID)
        {
            if (string.IsNullOrEmpty(empUID))
                throw new ArgumentNullException(nameof(empUID));

            return await _productFeedbackDL.GetByEmpUID(empUID);
        }

        private void ValidateProductFeedback(IProductFeedback productFeedback)
        {
            if (string.IsNullOrEmpty(productFeedback.UID))
                throw new ArgumentException("UID is required", nameof(productFeedback));

            if (string.IsNullOrEmpty(productFeedback.StoreUID))
                throw new ArgumentException("StoreUID is required", nameof(productFeedback));

            if (string.IsNullOrEmpty(productFeedback.SKUUID))
                throw new ArgumentException("SKUUID is required", nameof(productFeedback));

            if (productFeedback.ExecutionTime == DateTime.MinValue)
                throw new ArgumentException("ExecutionTime is required", nameof(productFeedback));
        }
    }
} 