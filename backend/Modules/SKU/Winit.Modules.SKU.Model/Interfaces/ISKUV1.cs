namespace Winit.Modules.SKU.Model.Interfaces;

public interface ISKUV1 : ISKU
{
    public string? L1 { get; set; }
    public string? L2 { get; set; }
    public string? L3 { get; set; }
    public string? L4 { get; set; }
    public string? L5 { get; set; }
    public string? L6 { get; set; }
    public string ModelCode { get; set; }
    public int Year { get; set; }
    public string Type { get; set; }
    public string ProductType { get; set; }
    public string Category { get; set; }
    public string Tonnage { get; set; }
    public string Capacity { get; set; }
    public string StarRating { get; set; }
    public int ProductCategoryId { get; set; }
    public string ProductCategoryName { get; set; }
    public string ItemSeries { get; set; }
    public string HSNCode { get; set; }
    public bool IsAvailableInApMaster { get; set; }
    HashSet<string> FilterKeys { get; set; }
}
