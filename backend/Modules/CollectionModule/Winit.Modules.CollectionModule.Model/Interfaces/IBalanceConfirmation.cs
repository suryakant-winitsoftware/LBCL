using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface IBalanceConfirmation : IBaseModelV2
    {
        public string? Org { get; set; }
        public string? Store { get; set; }
        public DateTime GeneratedOn { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal TotalDisputeAmount { get; set; }
        public string? Status { get; set; }
        public string? Comments { get; set; }
        public string? OTPCode { get; set; }
        public string? RequestByJobPositionUID { get; set; }
        public string? RequestByEmpUID { get; set; }
        public string? DisputeConfirmationByJobPositionUID { get; set; }
        public string? DisputeconfirmationByEmpUID { get; set; }
        public DateTime ConfirmationOrDisputeRequestTime { get; set; }
        public DateTime ConfirmationRequestTimeOrDisputeConfirmationTime { get; set; }
    }
}
