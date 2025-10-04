using System;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Promotion.Model.Interfaces
{
    public interface IPromotionVolumeCap : IBaseModel
    {
        string PromotionUID { get; set; }
        bool Enabled { get; set; }
        string OverallCapType { get; set; }
        decimal OverallCapValue { get; set; }
        decimal OverallCapConsumed { get; set; }
        decimal InvoiceMaxDiscountValue { get; set; }
        int InvoiceMaxQuantity { get; set; }
        int InvoiceMaxApplications { get; set; }
        string CreatedBy { get; set; }
        DateTime CreatedTime { get; set; }
        string ModifiedBy { get; set; }
        DateTime ModifiedTime { get; set; }
        DateTime ServerAddTime { get; set; }
        DateTime ServerModifiedTime { get; set; }
    }
}