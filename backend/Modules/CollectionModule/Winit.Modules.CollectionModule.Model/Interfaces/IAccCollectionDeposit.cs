using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.CollectionModule.Model.Classes;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface IAccCollectionDeposit :  IBaseModel
    {
        public string EmpUID { get; set; }
        public string JobPositionUID { get; set; }
        public string RequestNo { get; set; }
        public DateTime RequestDate { get; set; }
        public decimal Amount { get; set; }
        public string BankUID { get; set; }
        public string Branch { get; set; }
        public string Notes { get; set; }
        public string Comments { get; set; }
        public string Status { get; set; }
        public string ReceiptNos { get; set; }
        public DateTime ApprovalDate { get; set; }
        public string ApprovedByEmpUID { get; set; }
    }
}
