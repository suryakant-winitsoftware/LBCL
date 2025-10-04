using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.Tax.BL.Interfaces;
using Winit.Modules.Tax.Model.Interfaces;

namespace Winit.Modules.Tax.BL.Interfaces
{
    public interface ITaxCalculator
    {
        void SetTaxCalculator(IServiceProvider serviceProvider, IAppSetting appSetting);
        List<string> GetApplicableTaxesByApplicableAt(Dictionary<string, ITax> taxDictionary, string applicableAt);
        Task<List<IAppliedTax>> CalculateTaxes(decimal price, List<string> ApplicableTaxes, Dictionary<string, ITax> taxDictionary,
            bool isPriceInclusiveOfTax = false);
    }
}
