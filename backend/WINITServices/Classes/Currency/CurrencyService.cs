using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WINITRepository.Interfaces.Products;
using WINITServices.Classes.Currency;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Settings.PostgreSQLSettingsRepository;

namespace WINITServices.Classes.Currency
{
    public class CurrencyService : CurrencyBaseService
    {
        public CurrencyService(WINITRepository.Interfaces.Currency.ICurrencyRepository currencyRepository) : base(currencyRepository)
        {

        }

        public async override Task<IEnumerable<WINITSharedObjects.Models.Currency>> GetCurrencyDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias)
        {
            return await _currencyRepository.GetCurrencyDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
        }

        public async override Task<WINITSharedObjects.Models.Currency> GetCurrencyById(string UID)
        {
            return await _currencyRepository.GetCurrencyById(UID);
        }
        public async override Task<WINITSharedObjects.Models.Currency> CreateCurrency(WINITSharedObjects.Models.Currency createCurrency)
        {
            return await _currencyRepository.CreateCurrency(createCurrency);
        }

        public async override Task<int> UpdateCurrency(WINITSharedObjects.Models.Currency updateCurrency)
        {
            return await _currencyRepository.UpdateCurrency(updateCurrency);
        }

        public async override Task<int> DeleteCurrency(string UID)
        {
            return await _currencyRepository.DeleteCurrency(UID);
        }
    }
}
