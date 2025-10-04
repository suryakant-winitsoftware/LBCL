using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface ICollectionAmount
    {
        public decimal CashAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ChequeAmount { get; set; }
        public string ChequeNo { get; set; }
        public decimal POSAmount { get; set; }
        public string ChequeBank { get; set; }
        public string POSBank { get; set; }
        public string OnlineBank { get; set; }
        public string POSNo { get; set; }
        public decimal OnlineAmount { get; set; }
        public string OnlineNo { get; set; }
        public string ChequeBranchName { get; set; } 
        public string POSBranchName { get; set; }
        public string OnlineBranchName { get; set; }
        public bool Dense_CheckBox { get; set; }
        public bool CheckBoxState { get; set; }
        public DateTime ChequeTransferDate { get; set; } 
        public DateTime POSTransferDate { get; set; } 
        public DateTime OnlineTransferDate { get; set; }
        public bool CashOnAccount { get; set; } 
        public bool ChequeOnAccount { get; set; }
        public bool POSOnAccount { get; set; }
        public bool OnlineOnAccount { get; set; }

        public bool CashReceipt { get; set; } 
        public bool ChequeReceipt { get; set; }
        public bool POSReceipt { get; set; }
        public bool OnlineReceipt { get; set; }

        public bool LockReceipt { get; set; } 
        public bool LockOnAccountReceipt { get; set; } 
        public bool LockReceiptCondition { get; set; }
        public bool IsAutoAllocate { get; set; }
        public decimal TotalInputAmount => CashAmount + ChequeAmount + POSAmount + OnlineAmount;
    }
}
