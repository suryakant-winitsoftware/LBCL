using System;
using Winit.Modules.Target.Model.Interfaces;

namespace Winit.Modules.Target.Model.Classes
{
    public class Target : ITarget
    {
        public long Id { get; set; }
        public string UserLinkedType { get; set; } = string.Empty;
        public string UserLinkedUid { get; set; } = string.Empty;
        public string? CustomerLinkedType { get; set; }
        public string? CustomerLinkedUid { get; set; }
        public string? ItemLinkedItemType { get; set; }
        public string? ItemLinkedItemUid { get; set; }
        public int TargetMonth { get; set; }
        public int TargetYear { get; set; }
        public decimal TargetAmount { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public DateTime? CreatedTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class TargetUploadRequest
    {
        public string FileName { get; set; } = string.Empty;
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
        public string UploadedBy { get; set; } = string.Empty;
    }

    public class TargetFilter
    {
        public string? UserLinkedType { get; set; }
        public string? UserLinkedUid { get; set; }
        public string? CustomerLinkedType { get; set; }
        public string? CustomerLinkedUid { get; set; }
        public string? ItemLinkedItemType { get; set; }
        public int? TargetMonth { get; set; }
        public int? TargetYear { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class TargetSummary
    {
        public string UserLinkedUid { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? CustomerLinkedType { get; set; }
        public string? CustomerLinkedUid { get; set; }
        public string? CustomerName { get; set; }
        public int TargetMonth { get; set; }
        public int TargetYear { get; set; }
        public decimal TotalTarget { get; set; }
        public decimal CosmeticsTarget { get; set; }
        public decimal FMCGNonFoodTarget { get; set; }
        public decimal FMCGFoodTarget { get; set; }
    }

    public class PagedResult<T>
    {
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }
}