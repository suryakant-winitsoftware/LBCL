using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.SKU.BL.Interfaces
{
    public interface ISKUPriceViewModel : ITableGridViewModel
    {
        bool IsIndividualPricelist { get; set; }
        bool IsNew { get; set; }
        Winit.Modules.SKU.Model.Interfaces.ISKUPriceView SKUPriceView { get; }
        Task PopulateViewmodel(string priceListUID = null);
        Task<ApiResponse<string>> SaveSKUPrices(List<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> sKUPrices);
    }
}
