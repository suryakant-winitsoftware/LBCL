using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.Model.Classes;

namespace Winit.Modules.CollectionModule.BL.Interfaces
{
    public interface IApprovePaymentViewModel
    {
        public AccCollectionPaymentMode[] Bank { get; set; }
        public AccCollection[] ReversalData { get; set; }

        Task GetChequeDetails(string UID, string TargetUID);
        Task CheckReversalPossible(string UID);
        Task<Winit.Shared.Models.Common.ApiResponse<string>> ReceiptReversal(string UID, decimal ChequeAmount, string ChequeNo, string SessionUserCode, string ReasonforCancelation);
    }
}
