using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Bank.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Bank.BL.Interfaces
{
    public interface IViewBankDetailsViewModel
    {
        public List<ISelectionItem> CountrySelectionItems { get; set; }
        public List<IBank> BankDetailsList { get; set; }
        public IBank ViewBankDetails { get; set; }
        Task PopulateViewModel();
        Task PopulateBankViewDetailsByUID(string UID);
        Task<bool> CreateUpdateBankDetailsData(IBank bank, bool Operation);
        Task<string> DeleteVehicle(object uID);
        Task ApplyFilter(List<FilterCriteria> filterCriterias);
    }
}
