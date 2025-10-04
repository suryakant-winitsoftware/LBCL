using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface IAccBank
    {
        public int Id { get; set; }
        public string UID { get; set; }
        public string CNUID { get; set; }
        public decimal CNAmount { get; set; }
        public string BankName { get; set; }
        public string TargetUID { get; set; }
        public string BranchName { get; set; }
        public string PaymentType { get; set; }
        public string ChequeNo { get; set; }
        public string SessionUserCode { get; set; }
        public decimal ChequeAmount { get; set; }
        public DateTime ChequeDate { get; set; }
        public DateTime TransferDate { get; set; }
        public int Status { get; set; }
        public string Comments { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime ServerAddTime { get; set; }
        public DateTime ServerModifiedTime { get; set; }
        public string ReceiptUID { get; set; }
    }
}
