using System;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class PromotionVolumeCap : BaseModel, IPromotionVolumeCap
    {
        public string PromotionUID { get; set; }
        public bool Enabled { get; set; }
        public string OverallCapType { get; set; }
        public decimal OverallCapValue { get; set; }
        public decimal OverallCapConsumed { get; set; }
        public decimal InvoiceMaxDiscountValue { get; set; }
        public int InvoiceMaxQuantity { get; set; }
        public int InvoiceMaxApplications { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime ServerAddTime { get; set; }
        public DateTime ServerModifiedTime { get; set; }
    }
}