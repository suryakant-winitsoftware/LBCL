using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Scheme.Model.Interfaces;

namespace Winit.Modules.Scheme.Model.Classes
{
    public class Wallet:BaseModel,IWallet
    {
        public string Type { get; set; } // Or use an enum if there are predefined types
        public decimal BalanceAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal UsedAmount { get; set; }
        public string CurrencyUid { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public string LastTransactionType { get; set; } // Or use an enum if there are predefined types
        public string LastTransactionUid { get; set; }
        public string LinkedItemUid { get; set; }
        public string LinkedItemType { get; set; }
    }
}
