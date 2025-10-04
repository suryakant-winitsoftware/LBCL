using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.Classes;

namespace Winit.Modules.SKU.BL.Interfaces
{
    public interface IIndividualPriceListBaseViewModel
    {
        Task Save(List<SKUPrice> SKUPriceList);
    }
}
