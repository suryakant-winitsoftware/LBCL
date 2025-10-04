using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Scheme.Model.Interfaces;

namespace Winit.Modules.Scheme.Model.Classes
{
    public class WalletLedger:BaseModel,IWalletLedger
    {
        public string OrgUid { get; set; }
        public DateTime TransactionDateTime { get; set; }
        public string SourceType { get; set; } // Or use an enum if there are predefined source types
        public string SourceUid { get; set; }
        public string DocumentNumber { get; set; }
        public string Type { get; set; } // Or use an enum if there are predefined types
        public string CreditType { get; set; } // Or use an enum if there are predefined credit types
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public string LinkedItemUid { get; set; }
        public string LinkedItemType { get; set; }
    }
}
