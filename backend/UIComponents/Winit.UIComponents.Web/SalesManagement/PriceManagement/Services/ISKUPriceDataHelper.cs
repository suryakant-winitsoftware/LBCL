using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;

namespace Winit.UIComponents.Web.SalesManagement.PriceManagement.Services
{
    public interface ISKUPriceDataHelper
    {
        List<Winit.Modules.SKU.Model.Interfaces.ISKUV1> SKULIst { get; set; }

        Task GetAllProducts();
        Task<int> SaveUpdateOrDelete(string responseMethod, HttpMethod httpMethod, object obj=null);
        Task<ApiResponse<string>> CUD_SKUPrice(string responseMethod, HttpMethod httpMethod, object obj = null);
    }
}
