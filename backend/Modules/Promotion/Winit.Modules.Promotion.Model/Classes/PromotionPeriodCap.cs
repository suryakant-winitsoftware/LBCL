using System;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class PromotionPeriodCap : BaseModel, IPromotionPeriodCap
    {
        public string PromotionUID { get; set; }
        public string PeriodType { get; set; }
        public string CapType { get; set; }
        public decimal CapValue { get; set; }
        public decimal CapConsumed { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
    }
}