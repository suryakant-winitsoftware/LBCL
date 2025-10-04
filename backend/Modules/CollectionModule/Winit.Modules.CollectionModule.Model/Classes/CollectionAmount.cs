using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace Winit.Modules.CollectionModule.Model.Classes
{
    public class CollectionAmount : ICollectionAmount
    {
        public decimal CashAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ChequeAmount { get; set; }
        public string ChequeNo { get; set; }
        public string ChequeBank { get; set; } = "--Select Bank--";
        public string POSBank { get; set; } = "--Select Bank--";
        public string OnlineBank { get; set; } = "--Select Bank--";
        public decimal POSAmount { get; set; }
        public string POSNo { get; set; }
        public decimal OnlineAmount { get; set; }
        public string OnlineNo { get; set; }
        public string ChequeBranchName { get; set; }
        public string POSBranchName { get; set; }
        public string OnlineBranchName { get; set; }
        public bool Dense_CheckBox { get; set; } = false;
        public bool CheckBoxState { get; set; }
        public DateTime ChequeTransferDate { get; set; } = DateTime.Now.Date;
        public DateTime POSTransferDate { get; set; } = DateTime.Now.Date;
        public DateTime OnlineTransferDate { get; set; } = DateTime.Now.Date;
        public bool CashOnAccount { get; set; } = false;
        public bool ChequeOnAccount { get; set; } = false;
        public bool POSOnAccount { get; set; } = false;
        public bool OnlineOnAccount { get; set; } = false;
        public bool CashReceipt { get; set; } = false;
        public bool ChequeReceipt { get; set; } = false;
        public bool POSReceipt { get; set; } = false;
        public bool OnlineReceipt { get; set; } = false;
        public bool LockReceipt { get; set; } = true;
        public bool LockOnAccountReceipt { get; set; } = true;
        public bool LockReceiptCondition { get; set; } = false;
        public bool IsAutoAllocate { get; set; } = false;
        public decimal TotalInputAmount => CashAmount + ChequeAmount + POSAmount + OnlineAmount;
    }
}
