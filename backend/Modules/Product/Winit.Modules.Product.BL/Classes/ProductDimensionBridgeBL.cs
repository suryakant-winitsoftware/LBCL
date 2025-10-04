using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Product.BL.Classes
{
    public class ProductDimensionBridgeBL : ProductBaseBL, Interfaces.IProductDimensionBridgeBL
    {
        protected readonly DL.Interfaces.IProductDimensionBridgeDL _productdimensionbridgeRepository;
        public ProductDimensionBridgeBL(DL.Interfaces.IProductDimensionBridgeDL productdimensionbridgeRepository)
        {
            _productdimensionbridgeRepository = productdimensionbridgeRepository;
        }

        public async  Task<int> DeleteProductDimensionBridge(string UID)
        {
            return await _productdimensionbridgeRepository.DeleteProductDimensionBridge(UID);
        }
        public async  Task<int> CreateProductDimensionBridge(Winit.Modules.Product.Model.Interfaces.IProductDimensionBridge CreateProductDimensionBridge)
        {
            return await _productdimensionbridgeRepository.CreateProductDimensionBridge(CreateProductDimensionBridge);
        }
    }
}
