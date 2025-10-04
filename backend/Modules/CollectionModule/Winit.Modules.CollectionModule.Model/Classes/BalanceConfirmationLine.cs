using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace Winit.Modules.CollectionModule.Model.Classes
{
    public class BalanceConfirmationLine : BaseModelV2 ,IBalanceConfirmationLine
    {
        public string BalanceConfirmationUID { get; set; }
        public Int32 LineNumber { get; set; }
        public string? SchemeName { get; set; }
        public decimal EligibleAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public string? Description { get; set; }
        public decimal DisputeAmount { get; set; }
        public string? JobPositionUID { get; set; }
        public string? EmpUID { get; set; }

    }
}
