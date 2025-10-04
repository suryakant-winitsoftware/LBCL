using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.CollectionModule.Model.Interfaces;
using static System.Net.WebRequestMethods;

namespace Winit.Modules.CollectionModule.Model.Classes
{
    public class BalanceConfirmation : BaseModelV2 ,IBalanceConfirmation
    {
        public string? Org { get; set; }
        public string? Store { get; set; }
        public DateTime GeneratedOn { get; set; }
        private DateTime _startDate;
        public DateTime StartDate
        {
            get => _startDate == DateTime.MinValue ? DateTime.Now : _startDate;
            set => _startDate = value;
        }

        private DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate == DateTime.MinValue ? DateTime.Now : _endDate;
            set => _endDate = value;
        }
        public decimal OpeningBalance { get; set; }
        public string? Comments { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal TotalDisputeAmount { get; set; }
        public string? Status { get; set; }
        public string? OTPCode { get; set; }
        public DateTime ConfirmationOrDisputeRequestTime { get; set; }
        public DateTime ConfirmationRequestTimeOrDisputeConfirmationTime { get; set; }
        public string? RequestByJobPositionUID { get; set; }
        public string? RequestByEmpUID { get; set; }
        public string? DisputeConfirmationByJobPositionUID { get; set; }
        public string? DisputeconfirmationByEmpUID { get; set; }
    }
}
