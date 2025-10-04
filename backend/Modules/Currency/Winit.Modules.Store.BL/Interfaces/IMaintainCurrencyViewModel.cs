using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Currency.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Currency.BL.Interfaces
{
    public interface IMaintainCurrencyViewModel
    {
        public List<ISelectionItem> DigitsSelectionItems { get; set; }
        public List<ICurrency> CurrencyDetailsList { get; set; }
        public ICurrency ViewCurrencyDetails { get; set; }
        Task PopulateViewModel();
        Task PopulateCurrencyViewDetailsByUID(string UID);
        Task<bool> CreateUpdateCurrencyDetailsData(ICurrency bank, bool Operation);
        Task<string> DeleteCurrency(object uID);
        Task ApplyFilter(List<FilterCriteria> filterCriterias);
    }
}
