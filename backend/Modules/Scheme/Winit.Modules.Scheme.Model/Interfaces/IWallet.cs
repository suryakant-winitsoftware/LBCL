using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Scheme.Model.Interfaces
{
    public interface IWallet:IBaseModel
    {
        public string Type { get; set; }
        public decimal BalanceAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal UsedAmount { get; set; }
        public string CurrencyUid { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public string LastTransactionType { get; set; }
        public string LastTransactionUid { get; set; }
        public string LinkedItemUid { get; set; }
        public string LinkedItemType { get; set; }
    }
}
