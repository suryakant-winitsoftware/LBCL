using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.Tax.BL.Interfaces;
using Winit.Modules.Tax.DL.Interfaces;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;

namespace Winit.Modules.Tax.BL.Classes
{
    public class TaxCalculator : ITaxCalculator
    {
        private IServiceProvider _serviceProvider;
        IAppSetting _appSetting;
        public TaxCalculator()
        {
        }
        public void SetTaxCalculator(IServiceProvider serviceProvider, IAppSetting appSetting)
        {
            _serviceProvider = serviceProvider;
            _appSetting = appSetting;
        }
        public List<string> GetApplicableTaxesByApplicableAt(Dictionary<string, ITax> taxDictionary, string applicableAt)
        {
            if (taxDictionary is null) return [];
            // Filter the tax dictionary to get taxes applicable at the specified level
            var specificApplicableTaxes = taxDictionary
                .Where(tax => tax.Value.ApplicableAt.Equals(applicableAt, StringComparison.OrdinalIgnoreCase))
                .Select(tax => tax.Key)
                .ToList();

            return specificApplicableTaxes;
        }
        public async Task<List<IAppliedTax>> CalculateTaxes(decimal price, List<string> ApplicableTaxes, Dictionary<string, ITax> taxDictionary,
            bool isPriceInclusiveOfTax = false)
        {
            var appliedTaxes = new List<IAppliedTax>();

            // Adjust base price for inclusive tax calculation
            if (isPriceInclusiveOfTax)
            {
                // Adjust the price to calculate the base price (excluding taxes)
                decimal totalTaxRate = ApplicableTaxes
                    .Where(taxUID => !taxDictionary[taxUID].IsTaxOnTaxApplicable)
                    .Sum(taxUID => taxDictionary[taxUID].BaseTaxRate);

                // Adjusted price for base tax calculation
                price = CommonFunctions.RoundForSystem(price / (1 + (totalTaxRate / 100)), _appSetting.RoundOffDecimal);

            }

            // Apply base taxes
            foreach (var taxUID in ApplicableTaxes.Where(taxUID => !taxDictionary[taxUID].IsTaxOnTaxApplicable))
            {
                var taxInfo = taxDictionary[taxUID];
                var taxAmount = CommonFunctions.RoundForSystem(price * taxInfo.BaseTaxRate / 100, _appSetting.RoundOffDecimal);

                IAppliedTax appliedTax = _serviceProvider.CreateInstance<IAppliedTax>();
                appliedTax.TaxUID = taxUID;
                appliedTax.TaxRate = taxInfo.BaseTaxRate;
                appliedTax.Amount = taxAmount;
                appliedTax.IsTaxOnTaxApplicable = false;

                appliedTaxes.Add(appliedTax);
            }

            // Apply dependent taxes
            foreach (var taxUID in ApplicableTaxes.Where(taxUID => taxDictionary[taxUID].IsTaxOnTaxApplicable))
            {
                var taxInfo = taxDictionary[taxUID];

                if (taxInfo == null || string.IsNullOrEmpty(taxInfo.DependentTaxes))
                {
                    continue;
                }

                // Get the list of dependent tax UIDs
                var dependentTaxUIDs = taxInfo.DependentTaxes?.Split(',').Select(uid => uid.Trim()) ?? Enumerable.Empty<string>();

                // Sum the amounts of the dependent taxes
                var baseTaxAmount = appliedTaxes
                    .Where(tax => dependentTaxUIDs.Contains(tax.TaxUID))
                    .Sum(tax => tax.Amount);

                var taxAmount = CommonFunctions.RoundForSystem(baseTaxAmount * taxInfo.BaseTaxRate / 100, _appSetting.RoundOffDecimal);

                IAppliedTax appliedTax = _serviceProvider.CreateInstance<IAppliedTax>();
                appliedTax.TaxUID = taxUID;
                appliedTax.TaxRate = taxInfo.BaseTaxRate;
                appliedTax.Amount = taxAmount;
                appliedTax.IsTaxOnTaxApplicable = true;

                appliedTaxes.Add(appliedTax);
            }
            return appliedTaxes;

        }
    }
}
