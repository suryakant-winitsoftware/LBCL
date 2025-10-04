using System;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class PromotionHierarchyCap : BaseModel, IPromotionHierarchyCap
    {
        public string PromotionUID { get; set; }
        public string HierarchyType { get; set; }
        public string HierarchyUID { get; set; }
        public string HierarchyName { get; set; }
        public string CapType { get; set; }
        public decimal CapValue { get; set; }
        public decimal CapConsumed { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
    }
}