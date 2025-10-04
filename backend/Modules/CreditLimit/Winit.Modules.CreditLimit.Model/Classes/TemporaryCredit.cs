using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.CreditLimit.Model.Interfaces;

namespace Winit.Modules.CreditLimit.Model.Classes
{
    public class TemporaryCredit : BaseModel, ITemporaryCredit
    {
        public string? StoreUID { get; set; }
        public string? OrderNumber { get; set; }
        public string? RequestType { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveUpto { get; set; }
        public decimal? RequestAmountDays { get; set; }
        public string? Remarks { get; set; }
        public string? Status { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string DivisionOrgUID { get; set; }
        public string? CreditData { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime CalenderEndDate { get; set; }
        public int MaxDays { get; set; }
        public decimal CollectionMTD { get; set; } 
        public decimal CollectionBOM { get; set; }
    }
}
