using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Product.DL.Interfaces
{
    public interface IProductTypeBridgeDL
    {
        Task<int> CreateProductTypeBridge(Model.Interfaces.IProductTypeBridge CreateProductTypeBridge);
        Task<int> DeleteProductTypeBridgeByUID(String UID);

    }
}
