using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIComponents.Web.SalesManagement.PriceManagement.Services
{
    public interface ISKUPriceBaseViewModel
    {
        List<Winit.Modules.SKU.Model.Interfaces.ISKU> SKULIst { get; set; }

        Task GetAllProducts();
        Task<int> SaveUpdateOrDelete(string responseMethod, HttpMethod httpMethod, object obj);
    }
}
