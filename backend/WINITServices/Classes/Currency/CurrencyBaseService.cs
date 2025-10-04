using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Bank.PostgreSQLBankRepository;

namespace WINITServices.Classes.Currency
{
    public abstract class CurrencyBaseService : Interfaces.ICurrencyService
    {
        protected readonly WINITRepository.Interfaces.Currency.ICurrencyRepository _currencyRepository;
        public CurrencyBaseService(WINITRepository.Interfaces.Currency.ICurrencyRepository currencyRepository)
        {
            _currencyRepository = currencyRepository;
        }
      public abstract  Task<IEnumerable<WINITSharedObjects.Models.Currency>> GetCurrencyDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
               int pageSize, List<FilterCriteria> filterCriterias);

        public abstract Task<WINITSharedObjects.Models.Currency> GetCurrencyById(string UID);

        public abstract Task<WINITSharedObjects.Models.Currency> CreateCurrency(WINITSharedObjects.Models.Currency createCurrency);
        public abstract Task<int> UpdateCurrency(WINITSharedObjects.Models.Currency updateCurrency);
        public abstract Task<int> DeleteCurrency(string UID);

    }
}
