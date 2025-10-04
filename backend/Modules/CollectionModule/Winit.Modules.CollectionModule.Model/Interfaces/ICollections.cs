using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface ICollections 
    {
        public IAccCollection AccCollection { get; set; }
        public IAccCollectionPaymentMode AccCollectionPaymentMode { get; set; }
        public IAccStoreLedger AccStoreLedger { get; set; }
        public IAccCollectionSettlement AccCollectionSettlement { get; set; }
        public List<IAccCollectionAllotment> AccCollectionAllotment { get; set; }
        public List<IAccPayable> AccPayable { get; set; }
        public List<IAccReceivable> AccReceivable { get; set; }
        public List<IAccCollectionSettlementReceipts> AccCollectionSettlementReceipts { get; set; }
        public List<IAccCollectionCurrencyDetails> AccCollectionCurrencyDetails { get; set; }
    }
}
