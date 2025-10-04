using System;

namespace Winit.Modules.Target.Model.Interfaces
{
    public interface ITarget
    {
        long Id { get; set; }
        string UserLinkedType { get; set; }
        string UserLinkedUid { get; set; }
        string? CustomerLinkedType { get; set; }
        string? CustomerLinkedUid { get; set; }
        string? ItemLinkedItemType { get; set; }
        string? ItemLinkedItemUid { get; set; }
        int TargetMonth { get; set; }
        int TargetYear { get; set; }
        decimal TargetAmount { get; set; }
        string? Status { get; set; }
        string? Notes { get; set; }
        DateTime? CreatedTime { get; set; }
        string? CreatedBy { get; set; }
        DateTime? ModifiedTime { get; set; }
        string? ModifiedBy { get; set; }
    }
}