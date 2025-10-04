using SyncManagerModel.Base;
using SyncManagerModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerModel.Classes
{
    public class CustomerCreditLimit : SyncBaseModel,ICustomerCreditLimit
    {
        public long SyncLogId { get; set; }
        public string UID { get; set; }
        public string OrgUid { get; set; }
        public string CustomerNumber { get; set; }
        public decimal Division { get; set; }
        public decimal SalesOffice { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal MarginalCreditLimit { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveUpto { get; set; }
    }
}
