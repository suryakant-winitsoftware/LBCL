using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.ApprovalEngine.Model.Classes
{

    //Do not modify this class without Amit sir permission
    public class ApprovalType
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public bool IsNew { get; set; }
        public int OrderCount { get; set; }
        public decimal CreditLimit { get; set; }
        //new
        public string BroadCustomerClassification { get; set; }
        public string ClassificationType { get; set; }
        public string FirmType { get; set; }
        public string City { get; set; }
        public bool IsMSME { get; set; }
        public string CreatedBy { get; set; }
    }
}
