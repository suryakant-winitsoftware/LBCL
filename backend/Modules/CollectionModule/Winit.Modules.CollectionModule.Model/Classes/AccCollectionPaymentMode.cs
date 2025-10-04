using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace Winit.Modules.CollectionModule.Model.Classes
{
    public class AccCollectionPaymentMode : BaseModel, IAccCollectionPaymentMode
    {
        public string SessionUserCode { get; set; }
        public string ReceiptNumber { get; set; }
        public string StoreUID { get; set; }
        public string CheckListData { get; set; }
        public string Category { get; set; }
        public string AccCollectionUID { get; set; }
        public string BankUID { get; set; }
        public string Branch { get; set; }
        public string ChequeNo { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyUID { get; set; }
        public string Comments { get; set; }
        public string ApproveComments { get; set; }
        public string DefaultCurrencyUID { get; set; }
        public decimal DefaultCurrencyExchangeRate { get; set; }
        public decimal DefaultCurrencyAmount { get; set; }
        public DateTime? ChequeDate { get; set; }
        public string Status { get; set; }
        public DateTime? RealizationDate { get; set; }
    }
}
