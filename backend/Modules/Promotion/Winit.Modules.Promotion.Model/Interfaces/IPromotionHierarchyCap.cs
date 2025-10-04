using System;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Promotion.Model.Interfaces
{
    public interface IPromotionHierarchyCap : IBaseModel
    {
        string PromotionUID { get; set; }
        string HierarchyType { get; set; }
        string HierarchyUID { get; set; }
        string HierarchyName { get; set; }
        string CapType { get; set; }
        decimal CapValue { get; set; }
        decimal CapConsumed { get; set; }
        bool IsActive { get; set; }
        string CreatedBy { get; set; }
        DateTime CreatedTime { get; set; }
        string ModifiedBy { get; set; }
        DateTime ModifiedTime { get; set; }
    }
}