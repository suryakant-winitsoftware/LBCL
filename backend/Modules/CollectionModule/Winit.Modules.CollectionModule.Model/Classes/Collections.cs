using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace Winit.Modules.CollectionModule.Model.Classes
{
    public class Collections :  ICollections
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
        public class CustomResponse
        {
            public int StatusCode { get; set; }
            public int Value { get; set; }
            public string ResponseMessage { get; set; }
            public string ReceiptNumber { get; set; }
            public int RemainingAmount { get; set; }
            public DateTime? ServerAddTime { get; set; }
        }
        public class ExcepResponse
        {
            public int StatusCode { get; set; }
            public string ResponseMessage { get; set; }
            public DateTime? ServerAddTime { get; set; }
        }

    }
}
