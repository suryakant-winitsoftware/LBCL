using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface IEarlyPaymentDiscountConfiguration : IBaseModelV3
    {
        public string Sales_Org { get; set; }
        public string Applicable_Type { get; set; }
        public string Applicable_Code { get; set; }
        public string Payment_Mode { get; set; }
        public int Advance_Paid_Days { get; set; }
        public string Discount_Type { get; set; }
        public decimal Discount_Value { get; set; }
        public bool IsActive { get; set; }
        public DateTime Valid_From { get; set; }
        public DateTime Valid_To { get; set; }
        public bool Applicable_OnPartial_Payments { get; set; }
        public bool Applicable_OnOverDue_Customers { get; set; }
    }
}
