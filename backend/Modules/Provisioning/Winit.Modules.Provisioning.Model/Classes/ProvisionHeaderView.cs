using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Provisioning.Model.Interfaces;

namespace Winit.Modules.Provisioning.Model.Classes
{
    public class ProvisionHeaderView : BaseModel , IProvisionHeaderView
    {
        public string OrgUID { get; set; }
        public string City { get; set; }
        public decimal HoAmount { get; set; }
        public decimal P2Amount { get; set; }
        public decimal P3Amount { get; set; }
        public decimal P4Amount { get; set; }
        public decimal BalanceAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal UsedAmount { get; set; }
    }
}
