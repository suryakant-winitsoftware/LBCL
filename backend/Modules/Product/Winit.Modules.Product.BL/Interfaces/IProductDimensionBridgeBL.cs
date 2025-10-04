using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Product.BL.Interfaces
{
    public interface IProductDimensionBridgeBL
    {
        Task<int> DeleteProductDimensionBridge(string UID);
        Task<int> CreateProductDimensionBridge(Winit.Modules.Product.Model.Interfaces.IProductDimensionBridge CreateProductDimensionBridge);
    }
}
