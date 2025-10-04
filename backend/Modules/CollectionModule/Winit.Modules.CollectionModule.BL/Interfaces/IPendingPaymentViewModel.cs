using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.Model.Classes;

namespace Winit.Modules.CollectionModule.BL.Interfaces
{
    public interface IPendingPaymentViewModel
    {
        public AccCollectionPaymentMode[] Bank { get; set; }
        Task GetChequeDetails(string UID, string TargetUID);
        Task<bool> UpdateFields_Data(string UID, string BankName, string Branch, string ReferenceNumber);
        Task<Winit.Shared.Models.Common.ApiResponse<string>> OnClickSettleReject(string UID, string Button, string Comments1, string SessionUserCode, string CashNumber);
    }
}
