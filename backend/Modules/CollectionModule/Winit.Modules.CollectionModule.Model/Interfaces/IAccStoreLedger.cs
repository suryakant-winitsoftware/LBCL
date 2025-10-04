using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface IAccStoreLedger : IBaseModel
    {
        public string SessionUserCode { get; set; }

        public int ID { get; set; }

       

        public decimal PaidAmount { get; set; }
        public DateTime? CreatedTime { get; set; }

       

        public string SourceType { get; set; }

        public string SourceUID { get; set; }

        public string CreditType { get; set; }

        public string OrgUID { get; set; }

        public string StoreUID { get; set; }

        public string DefaultCurrencyUID { get; set; }

        public string DocumentNumber { get; set; }

        public decimal DefaultCurrencyExchangeRate { get; set; }

        public decimal DefaultCurrencyAmount { get; set; }

        public decimal Amount { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal Balance { get; set; }

        public DateTime? TransactionDateTime { get; set; }

        public decimal CollectedAmount { get; set; }

        public string CurrencyUID { get; set; }

        public string Comments { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public int TotalBalanceAmount { get; set; }

        public string AccColelctionUID { get; set; }
    }
}
