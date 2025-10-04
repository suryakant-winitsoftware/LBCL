using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Product.BL.Classes
{
    public class ProductTypeBridgeBL : ProductBaseBL, Interfaces.IProductTypeBridgeBL
    {
        protected readonly DL.Interfaces.IProductTypeBridgeDL _productTypeBridgeDL;
        public ProductTypeBridgeBL(DL.Interfaces.IProductTypeBridgeDL productTypeBridgeDL)
        {
            _productTypeBridgeDL = productTypeBridgeDL;
        }
        public async  Task<int> CreateProductTypeBridge(Model.Interfaces.IProductTypeBridge createProductTypeBridge)
        {
            return await _productTypeBridgeDL.CreateProductTypeBridge(createProductTypeBridge);
        }

        public async  Task<int> DeleteProductTypeBridgeByUID(String UID)
        {
            return await _productTypeBridgeDL.DeleteProductTypeBridgeByUID(UID);
        }



    }
}
