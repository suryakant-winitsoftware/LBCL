using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Product.BL.Interfaces
{
    public interface IProductTypeBridgeBL
    {
        Task<int> CreateProductTypeBridge(Model.Interfaces.IProductTypeBridge createProductTypeBridge);
        Task<int> DeleteProductTypeBridgeByUID(String UID);

    }
}
