using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Merchandiser.BL.Interfaces;
using Winit.Modules.Merchandiser.DL.Interfaces;
using Winit.Modules.Merchandiser.Model.Interfaces;

namespace Winit.Modules.Merchandiser.BL.Classes
{
    public class ProductSamplingBL : IProductSamplingBL
    {
        private readonly IProductSamplingDL _productSamplingDL;

        public ProductSamplingBL(IProductSamplingDL productSamplingDL)
        {
            _productSamplingDL = productSamplingDL;
        }

        public async Task<IProductSampling> GetByUID(string uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                throw new ArgumentException("UID cannot be null or empty", nameof(uid));
            }

            return await _productSamplingDL.GetByUID(uid);
        }

        public async Task<List<IProductSampling>> GetAll()
        {
            return await _productSamplingDL.GetAll();
        }

        public async Task<bool> Insert(IProductSampling productSampling)
        {
            if (!await Validate(productSampling))
            {
                throw new ArgumentException("Invalid product sampling data");
            }

            return await _productSamplingDL.Insert(productSampling);
        }

        public async Task<bool> Update(IProductSampling productSampling)
        {
            if (!await Validate(productSampling))
            {
                throw new ArgumentException("Invalid product sampling data");
            }

            var existing = await GetByUID(productSampling.UID);
            if (existing == null)
            {
                throw new ArgumentException($"Product sampling with UID {productSampling.UID} not found");
            }

            return await _productSamplingDL.Update(productSampling);
        }

        public async Task<bool> Delete(string uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                throw new ArgumentException("UID cannot be null or empty", nameof(uid));
            }

            var existing = await GetByUID(uid);
            if (existing == null)
            {
                throw new ArgumentException($"Product sampling with UID {uid} not found");
            }

            return await _productSamplingDL.Delete(uid);
        }

        public async Task<List<IProductSampling>> GetByStoreUID(string storeUID)
        {
            if (string.IsNullOrEmpty(storeUID))
            {
                throw new ArgumentException("Store UID cannot be null or empty", nameof(storeUID));
            }

            return await _productSamplingDL.GetByStoreUID(storeUID);
        }

        public async Task<List<IProductSampling>> GetByRouteUID(string routeUID)
        {
            if (string.IsNullOrEmpty(routeUID))
            {
                throw new ArgumentException("Route UID cannot be null or empty", nameof(routeUID));
            }

            return await _productSamplingDL.GetByRouteUID(routeUID);
        }

        public async Task<List<IProductSampling>> GetByEmpUID(string empUID)
        {
            if (string.IsNullOrEmpty(empUID))
            {
                throw new ArgumentException("Employee UID cannot be null or empty", nameof(empUID));
            }

            return await _productSamplingDL.GetByEmpUID(empUID);
        }

        public async Task<bool> Validate(IProductSampling productSampling)
        {
            if (productSampling == null)
            {
                return false;
            }

            // Validate required fields
            if (string.IsNullOrEmpty(productSampling.UID) ||
                string.IsNullOrEmpty(productSampling.CreatedBy) ||
                string.IsNullOrEmpty(productSampling.RouteUID) ||
                string.IsNullOrEmpty(productSampling.JobPositionUID) ||
                string.IsNullOrEmpty(productSampling.EmpUID) ||
                string.IsNullOrEmpty(productSampling.StoreUID) ||
                string.IsNullOrEmpty(productSampling.SKUUID))
            {
                return false;
            }

            // Validate execution time (must not be default)
            if (productSampling.ExecutionTime == default)
            {
                return false;
            }

            // Validate numeric fields
            if (productSampling.SellingPrice < 0 ||
                productSampling.UnitUsed < 0 ||
                productSampling.UnitSold < 0 ||
                productSampling.NoOfCustomerApproached < 0)
            {
                return false;
            }

            return true;
        }
    }
} 