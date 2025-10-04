using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Contact.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.CollectionModule.BL.Interfaces
{
    public interface IBalanceConfirmationViewModel
    {
        public List<FilterCriteria> FilterCriterias { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }
        public List<IStoreStatement> StoreStatementDetails { get; set; }
        public IBalanceConfirmation BalanceConfirmationDetails { get; set; }
        public List<IBalanceConfirmation> BalanceConfirmationListDetails { get; set; }
        public List<IBalanceConfirmationLine> BalanceConfirmationLineDetails { get; set; }
        public string imageSrc { get; set; }
        public IContact ContactDetails { get; set; }
        Task OnSortApply(SortCriteria sortCriteria);
        Task<List<IStoreStatement>> GetStoreStatementData(); 
        Task<bool> InsertDisputeRecords(List<IBalanceConfirmationLine> balanceConfirmationLine);
        Task GetBalanceConfirmationTableDetails(string StoreUID);
        Task GetBalanceConfirmationTableListDetails();
        Task GetBalanceConfirmationLineTableDetails(string UID);
        Task<bool> UpdateDisputeResolved(IBalanceConfirmation balanceConfirmation);
        Task<bool> UpdateBalanceConfirmation(IBalanceConfirmation balanceConfirmation);

        Task SendSms(string Otp, string MobileNumber);
        Task PopulateViewModel();
        public string GetHtmlContent(string fileName);
    }
}
